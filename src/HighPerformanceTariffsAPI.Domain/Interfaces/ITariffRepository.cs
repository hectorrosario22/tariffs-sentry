using HighPerformanceTariffsAPI.Domain.Entities;

namespace HighPerformanceTariffsAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for tariff data access operations.
/// </summary>
public interface ITariffRepository
{
    /// <summary>
    /// Retrieves all active tariff records with pagination support.
    /// </summary>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="offset">Number of records to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active tariff records</returns>
    Task<IEnumerable<Tariff>> GetAllAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tariff record by its ID.
    /// </summary>
    /// <param name="id">Tariff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tariff record or null if not found</returns>
    Task<Tariff?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active tariff records by base currency code.
    /// </summary>
    /// <param name="baseCurrency">Base currency code (e.g., "USD", "EUR")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active tariffs for the base currency</returns>
    Task<IEnumerable<Tariff>> GetByBaseCurrencyAsync(string baseCurrency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of active tariff records.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of active records</returns>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
