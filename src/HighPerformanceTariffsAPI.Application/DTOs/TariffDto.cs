namespace HighPerformanceTariffsAPI.Application.DTOs;

/// <summary>
/// Data transfer object for currency pair exchange rate information.
/// </summary>
public class TariffDto
{
    /// <summary>
    /// Unique identifier for the tariff.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Base currency code in ISO 4217 format (e.g., "USD", "EUR", "GBP").
    /// </summary>
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Target currency code in ISO 4217 format (e.g., "USD", "EUR", "GBP").
    /// </summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Exchange rate from base currency to target currency.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date when the tariff becomes effective.
    /// </summary>
    public DateOnly EffectiveDate { get; set; }
}
