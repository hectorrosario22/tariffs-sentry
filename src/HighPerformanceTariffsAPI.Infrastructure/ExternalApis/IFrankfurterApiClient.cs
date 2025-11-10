using Refit;
using HighPerformanceTariffsAPI.Infrastructure.ExternalApis.Models;

namespace HighPerformanceTariffsAPI.Infrastructure.ExternalApis;

/// <summary>
/// Refit client for Frankfurter API to fetch exchange rates.
/// </summary>
public interface IFrankfurterApiClient
{
    /// <summary>
    /// Gets the latest exchange rates for all available currencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing base currency, date, and rates</returns>
    [Get("/v1/latest")]
    Task<FrankfurterRatesResponse> GetLatestRatesAsync(CancellationToken cancellationToken = default);
}
