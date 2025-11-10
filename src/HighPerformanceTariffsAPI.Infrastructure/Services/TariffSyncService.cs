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

            // Check if we already have active data for today
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var hasActiveDataForToday = await dbContext.Tariffs
                .AnyAsync(t => t.EffectiveDate == today && t.IsActive, cancellationToken);

            if (hasActiveDataForToday)
            {
                logger.LogInformation("Active tariffs already synced for {Date}. Skipping synchronization", today);
                return;
            }

            // Step 1: Fetch all available currencies
            logger.LogInformation("Fetching available currencies from Frankfurter API");
            var currencies = await frankfurterClient.GetCurrenciesAsync(cancellationToken);

            if (currencies == null || currencies.Count == 0)
            {
                logger.LogWarning("No currencies returned from Frankfurter API");
                return;
            }

            var currencyCodes = currencies.Keys.ToList();
            logger.LogInformation("Found {Count} currencies to process", currencyCodes.Count);

            // Step 2: Deactivate all previous active tariffs (to maintain history)
            logger.LogInformation("Deactivating previous active tariff records");
            var previousActiveTariffs = await dbContext.Tariffs
                .Where(t => t.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var tariff in previousActiveTariffs)
            {
                tariff.IsActive = false;
                tariff.UpdatedAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Deactivated {Count} previous tariff records", previousActiveTariffs.Count);

            // Step 3: Fetch rates for each currency and build complete currency pair matrix
            var allTariffs = new List<Tariff>();
            var effectiveDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var processedCount = 0;

            foreach (var baseCurrency in currencyCodes)
            {
                try
                {
                    logger.LogInformation("Fetching rates for base currency: {BaseCurrency} ({Current}/{Total})",
                        baseCurrency, ++processedCount, currencyCodes.Count);

                    var response = await frankfurterClient.GetLatestRatesWithBaseAsync(baseCurrency, cancellationToken);

                    if (response?.Rates == null || response.Rates.Count == 0)
                    {
                        logger.LogWarning("No rates returned for base currency {BaseCurrency}", baseCurrency);
                        continue;
                    }

                    // Add all target currencies for this base
                    foreach (var rate in response.Rates)
                    {
                        allTariffs.Add(new Tariff
                        {
                            BaseCurrency = baseCurrency,
                            TargetCurrency = rate.Key,
                            Rate = rate.Value,
                            EffectiveDate = effectiveDate,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Small delay to avoid rate limiting
                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching rates for base currency {BaseCurrency}", baseCurrency);
                }
            }

            // Step 4: Remove duplicates
            var uniqueTariffs = allTariffs
                .GroupBy(t => new { t.BaseCurrency, t.TargetCurrency, t.EffectiveDate })
                .Select(g => g.First())
                .ToList();

            logger.LogInformation("Inserting {Count} unique tariff records (removed {Duplicates} duplicates)",
                uniqueTariffs.Count,
                allTariffs.Count - uniqueTariffs.Count);

            // Step 5: Insert into database
            await dbContext.Tariffs.AddRangeAsync(uniqueTariffs, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Step 6: Invalidate cache using pattern matching
            logger.LogInformation("Invalidating cache with pattern 'tariffs:all:*'");
            await cacheProvider.RemoveByPatternAsync("tariffs:all:*", cancellationToken);

            logger.LogInformation(
                "Successfully synchronized {Count} tariff records for date {Date} across {CurrencyCount} currencies",
                uniqueTariffs.Count,
                effectiveDate,
                currencyCodes.Count);
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
