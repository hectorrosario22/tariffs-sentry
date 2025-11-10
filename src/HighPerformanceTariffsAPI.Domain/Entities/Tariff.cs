namespace HighPerformanceTariffsAPI.Domain.Entities;

/// <summary>
/// Represents a currency pair exchange rate record.
/// Each record stores the exchange rate from one currency (base) to another (target).
/// </summary>
public class Tariff
{
    /// <summary>
    /// Unique identifier for the tariff record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Base currency code in ISO 4217 format (e.g., "USD", "EUR", "GBP", "JPY").
    /// This is the currency from which the exchange rate is calculated.
    /// </summary>
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Target currency code in ISO 4217 format (e.g., "USD", "EUR", "GBP", "JPY").
    /// This is the currency to which the exchange rate is calculated.
    /// </summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Exchange rate from base currency to target currency.
    /// For example, if BaseCurrency=EUR and TargetCurrency=USD, Rate=1.1571 means 1 EUR = 1.1571 USD.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date when the tariff becomes effective.
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// Indicates whether this tariff record is currently active.
    /// Only active records should be returned by the API.
    /// Historical records are marked as inactive for data retention.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp of record creation.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of last update.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
