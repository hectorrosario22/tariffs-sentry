using HighPerformanceTariffsAPI.Application.Services;
using HighPerformanceTariffsAPI.Domain.Interfaces;
using HighPerformanceTariffsAPI.Infrastructure.Caching;
using HighPerformanceTariffsAPI.Infrastructure.Data;
using HighPerformanceTariffsAPI.Infrastructure.ExternalApis;
using HighPerformanceTariffsAPI.Infrastructure.Repositories;
using HighPerformanceTariffsAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Refit;
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

// Add Rate Limiting with named policies and bypass support
var strictPolicyLimit = builder.Configuration.GetValue<int>("RateLimiting:StrictPolicy:PermitLimit", 2);
var strictPolicyWindow = builder.Configuration.GetValue<int>("RateLimiting:StrictPolicy:WindowSeconds", 60);
var permissivePolicyLimit = builder.Configuration.GetValue<int>("RateLimiting:PermissivePolicy:PermitLimit", 20);
var permissivePolicyWindow = builder.Configuration.GetValue<int>("RateLimiting:PermissivePolicy:WindowSeconds", 60);
var bypassKey = builder.Configuration.GetValue<string>("RateLimiting:BypassKey") ?? "performance-test-bypass";

builder.Services.AddRateLimiter(options =>
{
    // StrictPolicy with bypass support
    options.AddPolicy("StrictPolicy", ctx =>
    {
        var bypassHeader = ctx.Request.Headers.TryGetValue("X-Bypass-RateLimit", out var headerValue)
            ? headerValue.ToString()
            : null;

        if (!string.IsNullOrEmpty(bypassHeader) && bypassHeader == bypassKey)
        {
            return RateLimitPartition.GetNoLimiter("bypass");
        }

        var ipAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            ipAddress, partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = strictPolicyLimit,
                Window = TimeSpan.FromSeconds(strictPolicyWindow),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    // PermissivePolicy with bypass support
    options.AddPolicy("PermissivePolicy", ctx =>
    {
        var bypassHeader = ctx.Request.Headers.TryGetValue("X-Bypass-RateLimit", out var headerValue)
            ? headerValue.ToString()
            : null;

        if (!string.IsNullOrEmpty(bypassHeader) && bypassHeader == bypassKey)
        {
            return RateLimitPartition.GetNoLimiter("bypass");
        }

        var ipAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            ipAddress, partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permissivePolicyLimit,
                Window = TimeSpan.FromSeconds(permissivePolicyWindow),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterTimeSpan)
            ? (int)retryAfterTimeSpan.TotalSeconds
            : 60;

        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit exceeded",
            message = $"Too many requests. Retry after {retryAfter} seconds",
            retryAfter,
            timestamp = DateTime.UtcNow
        }, cancellationToken);
    };
});

// Register DbContext with PostgreSQL
var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException("PostgreSQL connection string not configured");

builder.Services.AddDbContext<TariffsDbContext>(options =>
{
    options.UseNpgsql(postgresConnection);
});

// Register Infrastructure Services (Scoped for DbContext)
builder.Services.AddScoped<ITariffRepository, TariffRepository>();
builder.Services.AddScoped<ITariffService, TariffService>();

// Configure Frankfurter API Client with Refit
var frankfurterBaseUrl = builder.Configuration["ExternalApis:FrankfurterBaseUrl"]
    ?? throw new InvalidOperationException("Frankfurter base URL not configured");

builder.Services.AddRefitClient<IFrankfurterApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(frankfurterBaseUrl))
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Register Tariff Synchronization Hosted Service
builder.Services.AddHostedService<TariffSyncService>();

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
    builder.Services.AddSingleton<ICacheProvider, NullCacheProvider>();
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
app.MapScalarApiReference("/", options =>
{
    options.WithTitle("High Performance Tariffs API")
        .WithTheme(ScalarTheme.Default)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

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
    .Produces(429)
    .RequireRateLimiting("StrictPolicy")
    .WithOpenApi();

group.MapGet("/fast", GetTariffsFast)
    .WithName("GetTariffsFast")
    .WithDescription("Optimized endpoint using distributed cache")
    .Produces(200)
    .Produces(429)
    .RequireRateLimiting("PermissivePolicy")
    .WithOpenApi();

// This is a demo application, so we apply migrations at startup for convenience
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<TariffsDbContext>();
await dbContext.Database.MigrateAsync();

await app.RunAsync();

// Endpoint handlers
async Task<IResult> GetTariffsSlow(ITariffService service, string? @base = null, int limit = 500, int offset = 0, CancellationToken ct = default)
{
    var result = await service.GetTariffsSlowAsync(@base, limit, offset, ct);
    return Results.Ok(result);
}

async Task<IResult> GetTariffsFast(ITariffService service, string? @base = null, int limit = 500, int offset = 0, CancellationToken ct = default)
{
    var result = await service.GetTariffsCachedAsync(@base, limit, offset, ct);
    return Results.Ok(result);
}

// Null cache provider for when Redis is unavailable
public class NullCacheProvider : ICacheProvider
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class => Task.FromResult<T?>(null);
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
