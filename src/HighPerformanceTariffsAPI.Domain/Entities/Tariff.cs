namespace HighPerformanceTariffsAPI.Domain.Entities;

/// <summary>
/// Represents a tariff record with exchange rate information for a specific currency.
/// </summary>
public class Tariff
{
    /// <summary>
    /// Unique identifier for the tariff record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Currency code in ISO 4217 format (e.g., "USD", "EUR", "GBP", "JPY").
    /// Note: Field name is RegionCode for backward compatibility, but stores currency codes.
    /// </summary>
    public string RegionCode { get; set; } = string.Empty;

    /// <summary>
    /// Exchange rate relative to the base currency (typically EUR).
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
