using HighPerformanceTariffsAPI.Domain.Entities;
using HighPerformanceTariffsAPI.Domain.Interfaces;
using HighPerformanceTariffsAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HighPerformanceTariffsAPI.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Tariff entities using Entity Framework Core and PostgreSQL.
/// </summary>
public class TariffRepository : ITariffRepository
{
    private readonly TariffsDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TariffRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for tariff operations.</param>
    public TariffRepository(TariffsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Retrieves all tariffs with optional pagination.
    /// </summary>
    /// <param name="limit">The maximum number of records to return (default: 500).</param>
    /// <param name="offset">The number of records to skip (default: 0).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of tariff entities.</returns>
    public async Task<IEnumerable<Tariff>> GetAllAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a tariff by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tariff.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tariff entity if found; otherwise, null.</returns>
    public async Task<Tariff?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all tariffs for a specific region.
    /// </summary>
    /// <param name="regionCode">The region code to filter by (e.g., "US-CA", "EU-DE").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of tariff entities matching the region code.</returns>
    public async Task<IEnumerable<Tariff>> GetByRegionAsync(string regionCode, CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs
            .AsNoTracking()
            .Where(t => t.RegionCode == regionCode)
            .OrderBy(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the total count of all tariffs in the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total number of tariff records.</returns>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs
            .CountAsync(cancellationToken);
    }
}
