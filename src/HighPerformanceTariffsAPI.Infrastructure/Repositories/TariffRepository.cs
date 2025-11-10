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
    /// Retrieves all active tariffs with optional pagination and base currency filter.
    /// </summary>
    /// <param name="limit">The maximum number of records to return (default: 500).</param>
    /// <param name="offset">The number of records to skip (default: 0).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active tariff entities.</returns>
    public async Task<IEnumerable<Tariff>> GetAllAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.BaseCurrency)
            .ThenBy(t => t.TargetCurrency)
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
    /// Retrieves all active tariffs for a specific base currency.
    /// </summary>
    /// <param name="baseCurrency">The base currency code to filter by (e.g., "USD", "EUR").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active tariff entities matching the base currency.</returns>
    public async Task<IEnumerable<Tariff>> GetByBaseCurrencyAsync(string baseCurrency, CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs
            .AsNoTracking()
            .Where(t => t.BaseCurrency == baseCurrency && t.IsActive)
            .OrderBy(t => t.TargetCurrency)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the total count of active tariffs in the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total number of active tariff records.</returns>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs
            .Where(t => t.IsActive)
            .CountAsync(cancellationToken);
    }
}
