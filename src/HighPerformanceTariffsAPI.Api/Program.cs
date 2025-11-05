using HighPerformanceTariffsAPI.Application.Services;
using HighPerformanceTariffsAPI.Domain.Interfaces;
using HighPerformanceTariffsAPI.Infrastructure.Caching;
using HighPerformanceTariffsAPI.Infrastructure.Repositories;
using Scalar.AspNetCore;
using StackExchange.Redis;

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
    options.AddFixedWindowLimiter(policyName: "default", policy =>
    {
        policy.Window = TimeSpan.FromSeconds(60);
        policy.PermitLimit = 100;
        policy.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        policy.QueueLimit = 0;
    });
});

// Register Infrastructure Services
builder.Services.AddSingleton<ITariffRepository, MockTariffRepository>();
builder.Services.AddSingleton<ITariffService, TariffService>();

// Configure Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton(redis);
builder.Services.AddSingleton<ICacheProvider, RedisCacheProvider>();

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
