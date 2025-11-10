namespace HighPerformanceTariffsAPI.Infrastructure.ExternalApis.Models;

/// <summary>
/// Response model for Frankfurter API latest rates endpoint.
/// </summary>
public class FrankfurterRatesResponse
{
    /// <summary>
    /// Base currency code (e.g., "EUR").
    /// </summary>
    public string Base { get; set; } = string.Empty;

    /// <summary>
    /// Date of the rates (e.g., "2025-11-10").
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of currency codes and their exchange rates.
    /// </summary>
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
