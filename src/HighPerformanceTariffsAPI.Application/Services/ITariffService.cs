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
    /// <param name="baseCurrency">Optional base currency filter (e.g., "USD", "EUR")</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="offset">Number of records to skip for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<TariffsResponseDto> GetTariffsSlowAsync(string? baseCurrency = null, int limit = 500, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tariffs from cache (optimized with Cache-Aside pattern).
    /// </summary>
    /// <param name="baseCurrency">Optional base currency filter (e.g., "USD", "EUR")</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="offset">Number of records to skip for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<TariffsResponseDto> GetTariffsCachedAsync(string? baseCurrency = null, int limit = 500, int offset = 0, CancellationToken cancellationToken = default);
}
