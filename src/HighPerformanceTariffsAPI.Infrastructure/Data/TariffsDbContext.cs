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

            entity.Property(e => e.BaseCurrency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.TargetCurrency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.Rate)
                .IsRequired()
                .HasPrecision(18, 6);

            entity.Property(e => e.EffectiveDate)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            // Index for efficient base currency filtering
            entity.HasIndex(e => e.BaseCurrency)
                .HasDatabaseName("IX_Tariffs_BaseCurrency");

            // Index for efficient active record filtering
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Tariffs_IsActive");

            // Composite index for common query pattern (active records by base currency)
            entity.HasIndex(e => new { e.BaseCurrency, e.IsActive })
                .HasDatabaseName("IX_Tariffs_BaseCurrency_IsActive");

            // Composite index for efficient date-based queries
            entity.HasIndex(e => new { e.EffectiveDate, e.IsActive })
                .HasDatabaseName("IX_Tariffs_EffectiveDate_IsActive");
        });
    }
}
