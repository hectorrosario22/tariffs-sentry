using Refit;
using HighPerformanceTariffsAPI.Infrastructure.ExternalApis.Models;

namespace HighPerformanceTariffsAPI.Infrastructure.ExternalApis;

/// <summary>
/// Refit client for Frankfurter API to fetch exchange rates.
/// </summary>
public interface IFrankfurterApiClient
{
    /// <summary>
    /// Gets the latest exchange rates for all available currencies using the default base (EUR).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing base currency, date, and rates</returns>
    [Get("/v1/latest")]
    Task<FrankfurterRatesResponse> GetLatestRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest exchange rates with a specific base currency.
    /// </summary>
    /// <param name="baseCurrency">The base currency code (e.g., "USD", "EUR", "GBP")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing base currency, date, and rates relative to the base</returns>
    [Get("/v1/latest?base={baseCurrency}")]
    Task<FrankfurterRatesResponse> GetLatestRatesWithBaseAsync(string baseCurrency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available currency codes and their full names.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of currency codes and full names</returns>
    [Get("/v1/currencies")]
    Task<FrankfurterCurrenciesResponse> GetCurrenciesAsync(CancellationToken cancellationToken = default);
}
