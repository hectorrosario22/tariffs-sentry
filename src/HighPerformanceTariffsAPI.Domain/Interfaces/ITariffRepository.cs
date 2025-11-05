using HighPerformanceTariffsAPI.Domain.Entities;

namespace HighPerformanceTariffsAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for tariff data access operations.
/// </summary>
public interface ITariffRepository
{
    /// <summary>
    /// Retrieves all tariff records with pagination support.
    /// </summary>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="offset">Number of records to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tariff records</returns>
    Task<IEnumerable<Tariff>> GetAllAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tariff record by its ID.
    /// </summary>
    /// <param name="id">Tariff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tariff record or null if not found</returns>
    Task<Tariff?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tariff records by region code.
    /// </summary>
    /// <param name="regionCode">Region code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tariffs for the region</returns>
    Task<IEnumerable<Tariff>> GetByRegionAsync(string regionCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of tariff records.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count</returns>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
