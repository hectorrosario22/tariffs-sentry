namespace HighPerformanceTariffsAPI.Application.DTOs;

/// <summary>
/// Response object for tariff list queries.
/// </summary>
public class TariffsResponseDto
{
    /// <summary>
    /// Collection of tariff records.
    /// </summary>
    public IEnumerable<TariffDto> Data { get; set; } = [];

    /// <summary>
    /// Total count of all available records.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// When the data was retrieved.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cache hit indicator (for cached endpoint).
    /// </summary>
    public bool? FromCache { get; set; }

    /// <summary>
    /// When the data was cached (for cached endpoint).
    /// </summary>
    public DateTime? CachedAt { get; set; }
}
