using HighPerformanceTariffsAPI.Application.DTOs;
using HighPerformanceTariffsAPI.Domain.Interfaces;

namespace HighPerformanceTariffsAPI.Application.Services;

/// <summary>
/// Service for tariff operations with direct database access (simulated).
/// </summary>
public class TariffService : ITariffService
{
    private readonly ITariffRepository _repository;

    public TariffService(ITariffRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves tariffs directly from the repository with artificial delay.
    /// </summary>
    public async Task<TariffsResponseDto> GetTariffsSlowAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        // Simulate database latency
        await Task.Delay(300, cancellationToken);

        var tariffs = await _repository.GetAllAsync(limit, offset, cancellationToken);
        var total = await _repository.GetTotalCountAsync(cancellationToken);

        return new TariffsResponseDto
        {
            Data = MapToDto(tariffs),
            Total = total,
            Timestamp = DateTime.UtcNow,
            FromCache = false
        };
    }

    /// <summary>
    /// Retrieves tariffs from cache when available.
    /// </summary>
    public async Task<TariffsResponseDto> GetTariffsCachedAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        var tariffs = await _repository.GetAllAsync(limit, offset, cancellationToken);
        var total = await _repository.GetTotalCountAsync(cancellationToken);

        return new TariffsResponseDto
        {
            Data = MapToDto(tariffs),
            Total = total,
            Timestamp = DateTime.UtcNow,
            FromCache = true,
            CachedAt = DateTime.UtcNow.AddMinutes(-1) // Simulate cache age
        };
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
