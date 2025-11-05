using HighPerformanceTariffsAPI.Application.DTOs;

namespace HighPerformanceTariffsAPI.Application.Services;

/// <summary>
/// Service interface for tariff operations.
/// </summary>
public interface ITariffService
{
    /// <summary>
    /// Retrieves tariffs directly (simulated slow database query).
    /// </summary>
    Task<TariffsResponseDto> GetTariffsSlowAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tariffs from cache (optimized).
    /// </summary>
    Task<TariffsResponseDto> GetTariffsCachedAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default);
}
