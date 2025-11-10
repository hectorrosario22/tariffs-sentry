using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HighPerformanceTariffsAPI.Domain.Entities;
using HighPerformanceTariffsAPI.Domain.Interfaces;
using HighPerformanceTariffsAPI.Infrastructure.Data;
using HighPerformanceTariffsAPI.Infrastructure.ExternalApis;

namespace HighPerformanceTariffsAPI.Infrastructure.Services;

/// <summary>
/// Background service that synchronizes tariff data from Frankfurter API once per day.
/// </summary>
public class TariffSyncService(
    IServiceScopeFactory scopeFactory,
    IFrankfurterApiClient frankfurterClient,
    ICacheProvider cacheProvider,
    IConfiguration configuration,
    ILogger<TariffSyncService> logger) : IHostedService, IDisposable
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("TariffSyncService is starting");

        // Get configuration
        var syncHour = configuration.GetValue("TariffSync:SyncHour", 3);
        var syncMinute = configuration.GetValue("TariffSync:SyncMinute", 0);
        var enableOnStartup = configuration.GetValue("TariffSync:EnableOnStartup", true);

        // Calculate next execution time
        var now = DateTime.UtcNow;
        var nextRun = new DateTime(now.Year, now.Month, now.Day, syncHour, syncMinute, 0, DateTimeKind.Utc);

        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }

        var timeUntilNextRun = nextRun - now;
        logger.LogInformation("Next scheduled sync will run at {NextRun} UTC ({TimeUntilRun} from now)",
            nextRun, timeUntilNextRun);

        // Execute immediately on startup if enabled
        if (enableOnStartup)
        {
            logger.LogInformation("Executing initial sync on startup");
            Task.Run(() => ExecuteSyncAsync(cancellationToken), cancellationToken);
        }

        // Schedule daily timer
        _timer = new Timer(
            async _ => await ExecuteSyncAsync(CancellationToken.None),
            null,
            timeUntilNextRun,
            TimeSpan.FromDays(1));

        return Task.CompletedTask;
    }

    private async Task ExecuteSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting tariff synchronization");

            // Create scope to get DbContext (scoped service)
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TariffsDbContext>();

            // Check if we already have data for today
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var hasDataForToday = await dbContext.Tariffs
                .AnyAsync(t => t.EffectiveDate == today, cancellationToken);

            if (hasDataForToday)
            {
                logger.LogInformation("Tariffs already synced for {Date}. Skipping synchronization", today);
                return;
            }

            // Fetch latest rates from Frankfurter API
            logger.LogInformation("Fetching latest rates from Frankfurter API");
            var response = await frankfurterClient.GetLatestRatesAsync(cancellationToken);

            if (response?.Rates == null || response.Rates.Count == 0)
            {
                logger.LogWarning("No rates returned from Frankfurter API");
                return;
            }

            // Map response to Tariff entities
            var effectiveDate = DateOnly.Parse(response.Date);
            var tariffs = new List<Tariff>();

            // Add all currency rates from the response
            foreach (var rate in response.Rates)
            {
                tariffs.Add(new Tariff
                {
                    RegionCode = rate.Key, // Currency code (USD, GBP, JPY, etc.)
                    Rate = rate.Value,
                    EffectiveDate = effectiveDate,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Also include the base currency with rate 1.0
            tariffs.Add(new Tariff
            {
                RegionCode = response.Base, // EUR
                Rate = 1.0m,
                EffectiveDate = effectiveDate,
                CreatedAt = DateTime.UtcNow
            });

            // Remove duplicates (in case base currency is also in rates)
            var uniqueTariffs = tariffs
                .GroupBy(t => new { t.RegionCode, t.EffectiveDate })
                .Select(g => g.First())
                .ToList();

            logger.LogInformation("Inserting {Count} unique tariff records into database (removed {Duplicates} duplicates)",
                uniqueTariffs.Count,
                tariffs.Count - uniqueTariffs.Count);

            // Insert into database
            await dbContext.Tariffs.AddRangeAsync(uniqueTariffs, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Invalidate cache using pattern matching
            logger.LogInformation("Invalidating cache with pattern 'tariffs:all:*'");
            await cacheProvider.RemoveByPatternAsync("tariffs:all:*", cancellationToken);

            logger.LogInformation(
                "Successfully synchronized {Count} tariffs for date {Date}. Base currency: {Base}",
                uniqueTariffs.Count,
                response.Date,
                response.Base);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during tariff synchronization. Will retry on next scheduled run");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("TariffSyncService is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
