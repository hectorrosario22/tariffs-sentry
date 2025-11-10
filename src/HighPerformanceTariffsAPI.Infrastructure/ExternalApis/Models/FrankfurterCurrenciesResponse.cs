using System.Text.Json.Serialization;

namespace HighPerformanceTariffsAPI.Infrastructure.ExternalApis.Models;

/// <summary>
/// Response model for the Frankfurter /v1/currencies endpoint.
/// Contains a dictionary of currency codes and their full names.
/// </summary>
public class FrankfurterCurrenciesResponse : Dictionary<string, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FrankfurterCurrenciesResponse"/> class.
    /// </summary>
    public FrankfurterCurrenciesResponse() : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}
