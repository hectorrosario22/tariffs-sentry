namespace HighPerformanceTariffsAPI.Domain.Entities;

/// <summary>
/// Represents a tariff record with pricing information for a specific region.
/// </summary>
public class Tariff
{
    /// <summary>
    /// Unique identifier for the tariff record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Regional code (e.g., "US-CA", "US-TX", "EU-DE").
    /// </summary>
    public string RegionCode { get; set; } = string.Empty;

    /// <summary>
    /// Tariff rate in decimal format.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date when the tariff becomes effective.
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// Timestamp of record creation.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of last update.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
