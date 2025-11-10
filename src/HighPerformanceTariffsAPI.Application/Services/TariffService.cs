using HighPerformanceTariffsAPI.Application.DTOs;
using HighPerformanceTariffsAPI.Domain.Interfaces;

namespace HighPerformanceTariffsAPI.Application.Services;

/// <summary>
/// Service for tariff operations with direct database access (simulated).
/// </summary>
public class TariffService(
    ITariffRepository repository, ICacheProvider cacheProvider) : ITariffService
{

    /// <summary>
    /// Retrieves tariffs directly from the repository with artificial delay.
    /// </summary>
    public async Task<TariffsResponseDto> GetTariffsSlowAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        // Simulate database latency
        await Task.Delay(300, cancellationToken);

        var tariffs = await repository.GetAllAsync(limit, offset, cancellationToken);
        var total = await repository.GetTotalCountAsync(cancellationToken);

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
    public async Task<TariffsResponseDto> GetTariffsCachedAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        // 1. Generate cache key based on pagination parameters
        var cacheKey = $"tariffs:all:{limit}:{offset}";

        // 2. Try to get data from cache (Cache Hit scenario)
        var cachedData = await cacheProvider.GetAsync<TariffsResponseDto>(cacheKey, cancellationToken);
        if (cachedData != null)
        {
            cachedData.FromCache = true;
            cachedData.Timestamp = DateTime.UtcNow;
            return cachedData;
        }

        // 3. Cache Miss: query database
        var tariffs = await repository.GetAllAsync(limit, offset, cancellationToken);
        var total = await repository.GetTotalCountAsync(cancellationToken);

        // 4. Build response object
        var response = new TariffsResponseDto
        {
            Data = MapToDto(tariffs),
            Total = total,
            Timestamp = DateTime.UtcNow,
            FromCache = false,
            CachedAt = DateTime.UtcNow
        };

        // 5. Store in Redis cache with 5-minute TTL
        await cacheProvider.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);
        return response;
    }

    private static IEnumerable<TariffDto> MapToDto(IEnumerable<Domain.Entities.Tariff> tariffs)
    {
        return tariffs.Select(t => new TariffDto
        {
            Id = t.Id,
            RegionCode = t.RegionCode,
            Rate = t.Rate,
            EffectiveDate = t.EffectiveDate
        });
    }
}
