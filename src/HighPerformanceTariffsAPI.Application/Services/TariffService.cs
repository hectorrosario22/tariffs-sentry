using System.Diagnostics;
using HighPerformanceTariffsAPI.Application.DTOs;
using HighPerformanceTariffsAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HighPerformanceTariffsAPI.Application.Services;

/// <summary>
/// Service for tariff operations with direct database access (simulated).
/// </summary>
public class TariffService(
    ITariffRepository repository,
    ICacheProvider cacheProvider,
    ILogger<TariffService> logger) : ITariffService
{

    /// <summary>
    /// Retrieves tariffs directly from the repository with artificial delay.
    /// </summary>
    public async Task<TariffsResponseDto> GetTariffsSlowAsync(string? baseCurrency = null, int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        // Simulate database latency
        await Task.Delay(300, cancellationToken);

        IEnumerable<Domain.Entities.Tariff> tariffs;
        int total;

        if (!string.IsNullOrWhiteSpace(baseCurrency))
        {
            // Filter by base currency
            tariffs = await repository.GetByBaseCurrencyAsync(baseCurrency, cancellationToken);
            total = tariffs.Count();

            // Apply pagination manually for filtered results
            tariffs = tariffs.Skip(offset).Take(limit);
        }
        else
        {
            // Get all active tariffs with pagination
            tariffs = await repository.GetAllAsync(limit, offset, cancellationToken);
            total = await repository.GetTotalCountAsync(cancellationToken);
        }

        return new TariffsResponseDto
        {
            Data = MapToDto(tariffs).ToList(),
            Total = total,
            Timestamp = DateTime.UtcNow,
            FromCache = false
        };
    }

    /// <summary>
    /// Retrieves tariffs from cache when available, using Cache-Aside pattern.
    /// </summary>
    public async Task<TariffsResponseDto> GetTariffsCachedAsync(string? baseCurrency = null, int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        // 1. Generate cache key based on base currency and pagination parameters
        var cacheKey = string.IsNullOrWhiteSpace(baseCurrency)
            ? $"tariffs:all:{limit}:{offset}"
            : $"tariffs:base:{baseCurrency}:{limit}:{offset}";

        // 2. Try to get data from cache (Cache Hit scenario)
        var cachedData = await cacheProvider.GetAsync<TariffsResponseDto>(cacheKey, cancellationToken);
        if (cachedData != null)
        {
            logger.LogInformation("Cache Hit for key {CacheKey}. Data: {@CachedData}", cacheKey, cachedData);
            cachedData.FromCache = true;
            cachedData.Timestamp = DateTime.UtcNow;
            return cachedData;
        }
        
        logger.LogInformation("Cache Miss for key {CacheKey}. Fetching from database.", cacheKey);
        var stopwatch = Stopwatch.StartNew();

        // 3. Cache Miss: query database
        IEnumerable<Domain.Entities.Tariff> tariffs;
        int total;

        if (!string.IsNullOrWhiteSpace(baseCurrency))
        {
            // Filter by base currency
            tariffs = await repository.GetByBaseCurrencyAsync(baseCurrency, cancellationToken);
            total = tariffs.Count();

            // Apply pagination manually for filtered results
            tariffs = tariffs.Skip(offset).Take(limit);
        }
        else
        {
            // Get all active tariffs with pagination
            tariffs = await repository.GetAllAsync(limit, offset, cancellationToken);
            total = await repository.GetTotalCountAsync(cancellationToken);
        }
        
        stopwatch.Stop();
        logger.LogInformation("Database query for {CacheKey} completed in {DurationMs}ms", cacheKey, stopwatch.ElapsedMilliseconds);

        // 4. Build response object
        var response = new TariffsResponseDto
        {
            Data = MapToDto(tariffs),
            Total = total,
            Timestamp = DateTime.UtcNow,
            FromCache = false,
            CachedAt = DateTime.UtcNow
        };
        
        logger.LogInformation("Cache Miss for key {CacheKey}. Data fetched from DB and will be cached: {@ResponseData}", cacheKey, response);

        // 5. Store in Redis cache with 5-minute TTL
        await cacheProvider.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);
        return response;
    }

    private static IEnumerable<TariffDto> MapToDto(IEnumerable<Domain.Entities.Tariff> tariffs)
    {
        return tariffs.Select(t => new TariffDto
        {
            Id = t.Id,
            BaseCurrency = t.BaseCurrency,
            TargetCurrency = t.TargetCurrency,
            Rate = t.Rate,
            EffectiveDate = t.EffectiveDate
        });
    }
}
