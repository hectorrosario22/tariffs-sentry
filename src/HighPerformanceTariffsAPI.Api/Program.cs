using HighPerformanceTariffsAPI.Application.Services;
using HighPerformanceTariffsAPI.Domain.Interfaces;
using HighPerformanceTariffsAPI.Infrastructure.Caching;
using HighPerformanceTariffsAPI.Infrastructure.Repositories;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(60)
            }));
});

// Register Infrastructure Services
builder.Services.AddSingleton<ITariffRepository, MockTariffRepository>();
builder.Services.AddSingleton<ITariffService, TariffService>();

// Configure Redis
try
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
    var options = ConfigurationOptions.Parse(redisConnection);
    options.ConnectTimeout = 5000;
    options.AbortOnConnectFail = false;
    var redis = ConnectionMultiplexer.Connect(options);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    builder.Services.AddSingleton<ICacheProvider, RedisCacheProvider>();
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Redis connection failed: {ex.Message}");
    // Continue without Redis cache if connection fails
    builder.Services.AddSingleton<ICacheProvider>(new NullCacheProvider());
}

// Null cache provider for when Redis is unavailable
public class NullCacheProvider : ICacheProvider
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class => Task.FromResult<T?>(null);
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(false);
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();

// Map Scalar documentation to root
app.MapScalarApiReference(options =>
{
    options.WithTitle("High Performance Tariffs API")
        .WithTheme(ScalarTheme.Default)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
}).WithName("ScalarUI").WithOpenApi();

// Health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow })
    .WithName("HealthCheck")
    .WithOpenApi()
    .Produces(200);

// Tariffs API endpoints (v1)
var group = app.MapGroup("/api/v1/tariffs")
    .WithTags("Tariffs");

group.MapGet("/slow", GetTariffsSlow)
    .WithName("GetTariffsSlow")
    .WithDescription("Simulates direct database read with artificial latency")
    .Produces(200)
    .WithOpenApi();

group.MapGet("/fast", GetTariffsFast)
    .WithName("GetTariffsFast")
    .WithDescription("Optimized endpoint using distributed cache")
    .Produces(200)
    .WithOpenApi();

app.Run();

// Endpoint handlers
async Task<IResult> GetTariffsSlow(ITariffService service, int limit = 500, int offset = 0, CancellationToken ct = default)
{
    var result = await service.GetTariffsSlowAsync(limit, offset, ct);
    return Results.Ok(result);
}

async Task<IResult> GetTariffsFast(ITariffService service, int limit = 500, int offset = 0, CancellationToken ct = default)
{
    var result = await service.GetTariffsCachedAsync(limit, offset, ct);
    return Results.Ok(result);
}
