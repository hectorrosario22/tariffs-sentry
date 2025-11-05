namespace HighPerformanceTariffsAPI.Application.DTOs;

/// <summary>
/// Data transfer object for tariff information.
/// </summary>
public class TariffDto
{
    /// <summary>
    /// Unique identifier for the tariff.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Regional code for the tariff.
    /// </summary>
    public string RegionCode { get; set; } = string.Empty;

    /// <summary>
    /// The rate amount.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date when the tariff becomes effective.
    /// </summary>
    public DateOnly EffectiveDate { get; set; }
}
