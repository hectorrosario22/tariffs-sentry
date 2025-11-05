using HighPerformanceTariffsAPI.Domain.Entities;
using HighPerformanceTariffsAPI.Domain.Interfaces;

namespace HighPerformanceTariffsAPI.Infrastructure.Repositories;

/// <summary>
/// Mock repository for tariff data. Returns predefined mockup data.
/// </summary>
public class MockTariffRepository : ITariffRepository
{
    private readonly List<Tariff> _mockData;

    public MockTariffRepository()
    {
        _mockData = GenerateMockData();
    }

    public async Task<IEnumerable<Tariff>> GetAllAsync(int limit = 500, int offset = 0, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Simulate async operation
        return _mockData.Skip(offset).Take(limit);
    }

    public async Task<Tariff?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Simulate async operation
        return _mockData.FirstOrDefault(t => t.Id == id);
    }

    public async Task<IEnumerable<Tariff>> GetByRegionAsync(string regionCode, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Simulate async operation
        return _mockData.Where(t => t.RegionCode == regionCode);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Simulate async operation
        return _mockData.Count;
    }

    /// <summary>
    /// Generates 500 mock tariff records with realistic data.
    /// </summary>
    private static List<Tariff> GenerateMockData()
    {
        var regions = new[] { "US-CA", "US-TX", "US-NY", "US-FL", "US-PA", "EU-DE", "EU-FR", "EU-IT", "EU-ES", "AP-SG" };
        var tariffs = new List<Tariff>();
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 1; i <= 500; i++)
        {
            var regionCode = regions[i % regions.Length];
            var baseRate = 35m + (i % 100) * 0.50m;

            tariffs.Add(new Tariff
            {
                Id = i,
                RegionCode = regionCode,
                Rate = baseRate + (decimal)random.NextDouble() * 10,
                EffectiveDate = new DateOnly(2024, 1, 1),
                CreatedAt = DateTime.UtcNow.AddDays(-(500 - i)),
                UpdatedAt = DateTime.UtcNow.AddDays(-(250 - (i / 2)))
            });
        }

        return tariffs;
    }
}
