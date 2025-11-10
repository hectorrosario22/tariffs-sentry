using HighPerformanceTariffsAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HighPerformanceTariffsAPI.Infrastructure.Data;

/// <summary>
/// Database context for the Tariffs API, managing the Tariff entities and PostgreSQL connection.
/// </summary>
public class TariffsDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the Tariffs DbSet for database operations.
    /// </summary>
    public DbSet<Tariff> Tariffs { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="TariffsDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public TariffsDbContext(DbContextOptions<TariffsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the entity mappings and relationships for the database model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tariff>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RegionCode)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.Rate)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.EffectiveDate)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            // Index for efficient region-based queries
            entity.HasIndex(e => e.RegionCode)
                .HasDatabaseName("IX_Tariffs_RegionCode");
        });
    }

    /// <summary>
    /// Configures the context with seeding strategies for both synchronous and asynchronous scenarios.
    /// </summary>
    /// <param name="optionsBuilder">The options builder used to configure the context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder
            .UseSeeding((context, _) =>
            {
                // Synchronous seeding: Check if data exists, if not, seed the database
                var tariffsDbContext = (TariffsDbContext)context;
                if (!tariffsDbContext.Tariffs.Any())
                {
                    var tariffs = GenerateSeedData();
                    tariffsDbContext.Tariffs.AddRange(tariffs);
                    tariffsDbContext.SaveChanges();
                }
            })
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                // Asynchronous seeding: Check if data exists, if not, seed the database
                var tariffsDbContext = (TariffsDbContext)context;
                if (!await tariffsDbContext.Tariffs.AnyAsync(cancellationToken))
                {
                    var tariffs = GenerateSeedData();
                    tariffsDbContext.Tariffs.AddRange(tariffs);
                    await tariffsDbContext.SaveChangesAsync(cancellationToken);
                }
            });
    }

    /// <summary>
    /// Generates 500 seed tariff records with realistic data for testing and demonstration purposes.
    /// </summary>
    /// <returns>A list of 500 Tariff entities with varied regional data.</returns>
    private static List<Tariff> GenerateSeedData()
    {
        var regions = new[] { "US-CA", "US-TX", "US-NY", "US-FL", "US-PA", "EU-DE", "EU-FR", "EU-IT", "EU-ES", "AP-SG" };
        var tariffs = new List<Tariff>();
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 1; i <= 500; i++)
        {
            var regionCode = regions[i % regions.Length];
            var baseRate = 35m + (i % 100) * 0.50m;

            tariffs.Add(new Tariff
            {
                Id = i,
                RegionCode = regionCode,
                Rate = baseRate + (decimal)random.NextDouble() * 10,
                EffectiveDate = new DateOnly(2024, 1, 1),
                CreatedAt = DateTime.UtcNow.AddDays(-(500 - i)),
                UpdatedAt = DateTime.UtcNow.AddDays(-(250 - (i / 2)))
            });
        }

        return tariffs;
    }
}
