# AI Assistant Documentation - High Performance Tariffs API

This document contains complete instructions, guidelines, and context for AI assistants working on this project.

## Project Overview

**High Performance Tariffs API** is a production-grade portfolio project demonstrating modern software architecture and best practices. It showcases:

- Clean Architecture principles with 4-layer separation (Domain → Application → Infrastructure → Presentation)
- .NET 9 Minimal API with distributed caching and rate limiting
- Redis-backed distributed cache with graceful fallback
- Svelte 5 + Vite 7 reactive frontend with Tailwind CSS
- Docker/Podman containerization with multi-stage builds
- PostgreSQL 16 database (prepared for future use)
- Professional-grade documentation and portfolio standards

## Technology Stack

### Backend (.NET 9)
- **Framework:** ASP.NET Core 9.0 Minimal API
- **Architecture:** Clean Architecture (4 layers)
- **Caching:** StackExchange.Redis (Redis 7)
- **Rate Limiting:** Built-in AspNetCore.RateLimiting
- **Documentation:** Scalar.AspNetCore (OpenAPI UI)
- **Database:** PostgreSQL 16 with Entity Framework Core 9.0 + Npgsql
- **Migrations:** Automatic on startup via `MigrateAsync()` + `UseAsyncSeeding`

### Frontend (Svelte + Vite)
- **Framework:** Svelte 5.43.3
- **Build Tool:** Vite 7.2.0
- **Styling:** Tailwind CSS 3.4.3
- **Package Manager:** pnpm
- **Components:** 7 reusable Svelte components
- **Deployment:** nginx/http-server

### Infrastructure
- **Container Runtime:** Podman (rootless) or Docker
- **Orchestration:** Podman Compose / Docker Compose
- **Services:** PostgreSQL 16, Redis 7, .NET API, Svelte Demo
- **Networks:** Custom bridge network (tariffs-network)

## Project Structure

```
tariffs-sentry/
├── src/                                     # .NET 9 Backend
│   ├── HighPerformanceTariffsAPI.Domain/    # Domain Layer (no dependencies)
│   │   ├── Entities/Tariff.cs
│   │   └── Interfaces/
│   │       ├── ITariffRepository.cs
│   │       └── ICacheProvider.cs
│   ├── HighPerformanceTariffsAPI.Application/  # Application Layer
│   │   ├── DTOs/
│   │   │   ├── TariffDto.cs
│   │   │   └── TariffsResponseDto.cs
│   │   └── Services/
│   │       ├── ITariffService.cs
│   │       └── TariffService.cs
│   ├── HighPerformanceTariffsAPI.Infrastructure/ # Infrastructure Layer
│   │   ├── Data/
│   │   │   └── TariffsDbContext.cs              # EF Core DbContext with seeding
│   │   ├── Migrations/
│   │   │   ├── 20251107224156_InitialCreate.cs
│   │   │   ├── 20251107224156_InitialCreate.Designer.cs
│   │   │   └── TariffsDbContextModelSnapshot.cs
│   │   ├── Repositories/
│   │   │   └── TariffRepository.cs              # EF Core repository
│   │   └── Caching/
│   │       └── RedisCacheProvider.cs
│   └── HighPerformanceTariffsAPI.Api/      # Presentation Layer (Minimal API)
│       ├── Program.cs                      # Startup, DI, endpoints
│       ├── Dockerfile                      # Multi-stage Docker build
│       └── appsettings.json
├── demo/                                    # Svelte 5 Frontend
│   ├── src/
│   │   ├── App.svelte                      # Root component
│   │   ├── app.css                         # Global Tailwind styles
│   │   └── lib/
│   │       ├── Dashboard.svelte            # Main layout
│   │       ├── Header.svelte               # Navigation
│   │       ├── MetricsSection.svelte       # Metric cards container
│   │       ├── MetricCard.svelte           # Individual card
│   │       ├── PerformanceChart.svelte     # Performance testing
│   │       ├── ArchitectureInfo.svelte     # Documentation
│   │       └── NotificationArea.svelte     # Toast notifications
│   ├── package.json
│   ├── vite.config.js
│   ├── Dockerfile
│   └── README.md
├── compose.yml                              # Podman/Docker Compose
├── HighPerformanceTariffsAPI.sln           # Solution file
├── README.md                                # Main documentation
├── LICENSE                                  # MIT License
└── .env.example                            # Environment template

```

## Clean Architecture Layers

### Domain Layer (No External Dependencies)
**Location:** `src/HighPerformanceTariffsAPI.Domain/`

Contains core business entities and interfaces:
- **Tariff.cs:** Core entity with properties: Id, RegionCode, Rate, EffectiveDate, CreatedAt, UpdatedAt
- **ITariffRepository.cs:** Contract for data access operations
- **ICacheProvider.cs:** Contract for caching operations

Key principle: Zero external dependencies (no NuGet packages except unit test frameworks)

### Application Layer (Depends on Domain)
**Location:** `src/HighPerformanceTariffsAPI.Application/`

Contains business logic and data transfer objects:
- **TariffDto.cs:** DTO for transferring tariff data (Id, RegionCode, Rate, EffectiveDate)
- **TariffsResponseDto.cs:** API response wrapper (Data[], Total, Timestamp, FromCache, CachedAt)
- **TariffService.cs:** Service implementing:
  - `GetTariffsSlowAsync()` - Returns data with 300ms artificial delay
  - `GetTariffsCachedAsync()` - Returns cached data with metadata
- **ITariffService.cs:** Service contract

Key principle: Contains use cases and business logic. Depends only on Domain layer.

### Infrastructure Layer (Depends on Domain)
**Location:** `src/HighPerformanceTariffsAPI.Infrastructure/`

Contains external service integrations:

#### Database (Entity Framework Core 9.0)
- **TariffsDbContext.cs:**
  - DbSet: `Tariffs`
  - Provider: Npgsql.EntityFrameworkCore.PostgreSQL 9.0.2
  - OnModelCreating: Configures entity (HasKey, MaxLength, Precision(18,2), Index on RegionCode)
  - OnConfiguring: Implements `UseSeeding` + `UseAsyncSeeding`
  - Seeding: Generates 500 records with fixed seed (42) for reproducibility
  - Regions: US-CA, US-TX, US-NY, US-FL, US-PA, EU-DE, EU-FR, EU-IT, EU-ES, AP-SG
  - Execution: Automatic after `MigrateAsync()`
  - Verification: Only inserts if `!await Tariffs.AnyAsync()`

- **TariffRepository.cs:**
  - Implements ITariffRepository with EF Core
  - GetAllAsync(limit, offset): AsNoTracking + OrderBy + Skip + Take
  - GetByIdAsync(id): FirstOrDefaultAsync with AsNoTracking
  - GetByRegionAsync(regionCode): Where + OrderBy with AsNoTracking
  - GetTotalCountAsync(): CountAsync
  - Lifetime: Scoped (required for DbContext)

- **Migrations:**
  - 20251107224156_InitialCreate: Creates Tariffs table + IX_Tariffs_RegionCode index
  - TariffsDbContextModelSnapshot: Current model state for future migrations

#### Caching (Redis)
- **RedisCacheProvider.cs:**
  - Wraps StackExchange.Redis
  - Uses JSON serialization for cached objects
  - Silent error handling (returns null on failure)
  - Implements ICacheProvider interface

Key principle: Implements interfaces defined in Domain layer. Handles external services (PostgreSQL, Redis).

### Presentation Layer (API)
**Location:** `src/HighPerformanceTariffsAPI.Api/`

Contains HTTP endpoints and middleware configuration:
- **Program.cs:**
  - Service registration (Dependency Injection)
  - CORS configuration (AllowAll policy for demo)
  - Rate Limiting (100 requests per 60 seconds, IP-based)
  - Scalar API documentation at root path (`/`)
  - Health check endpoint (`/health`)
  - Tariffs API v1 endpoints:
    - `GET /api/v1/tariffs/slow` - Direct call with 300ms delay
    - `GET /api/v1/tariffs/fast` - Cached call with <10ms response
  - Redis fallback pattern (NullCacheProvider when Redis unavailable)

Key principle: Minimal API pattern without controllers. Depends on all layers below.

## API Endpoints

### Health Check
```
GET /health
Response: { "status": "healthy", "timestamp": "2025-11-05T10:30:00Z" }
```

### Tariffs API (v1)

#### Slow Endpoint (Direct Database Read)
```
GET /api/v1/tariffs/slow?limit=500&offset=0
Response: {
  "data": [
    { "id": 1, "regionCode": "US-CA", "rate": 45.50, "effectiveDate": "2025-01-01" },
    ...
  ],
  "total": 500,
  "timestamp": "2025-11-05T10:30:00Z",
  "fromCache": false,
  "cachedAt": null
}
Duration: ~300ms (simulated latency)
```

#### Fast Endpoint (Cached Read)
```
GET /api/v1/tariffs/fast?limit=500&offset=0
Response: {
  "data": [...],
  "total": 500,
  "timestamp": "2025-11-05T10:30:00Z",
  "fromCache": true,
  "cachedAt": "2025-11-05T10:25:00Z"
}
Duration: <10ms (from Redis cache)
```

#### Parameters
- `limit` (int, optional): Number of records to return (default: 500, max: 500)
- `offset` (int, optional): Number of records to skip (default: 0)

### API Documentation
```
GET /
Response: Scalar interactive API documentation UI
```

## Configuration

### Environment Variables (.env)
```
# Redis
ConnectionStrings__Redis=redis:6379

# API
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000

# PostgreSQL Database (fully integrated)
ConnectionStrings__PostgreSQL=Host=postgres;Port=5432;Database=tariffs;Username=postgres;Password=postgres
```

### Rate Limiting
- **Strategy:** Fixed Window (IP-based partitioning)
- **Limit:** 100 requests per 60 seconds
- **AutoReplenishment:** Enabled
- **Partition Key:** Client IP address (RemoteIpAddress)

### Cache Configuration
- **Provider:** Redis 7
- **Fallback:** NullCacheProvider (when Redis unavailable)
- **TTL:** 5 minutes (can be configured per operation)
- **Serialization:** JSON (System.Text.Json)

## Development Guidelines

### Backend Development

#### Code Organization
1. **Domain First:** Define entities and interfaces before implementation
2. **Dependency Injection:** Always register services in Program.cs
3. **Async/Await:** Use async patterns throughout
4. **Error Handling:** Implement graceful error handling with fallbacks
5. **Logging:** Use Console.WriteLine for development, structured logging for production

#### Common Patterns

**Adding a new service:**
```csharp
// 1. Define interface in Domain
public interface INewService
{
    Task<Result> GetDataAsync(CancellationToken ct);
}

// 2. Implement in Application or Infrastructure
public class NewService : INewService
{
    // Implementation
}

// 3. Register in Program.cs
builder.Services.AddSingleton<INewService, NewService>();

// 4. Use in endpoint
app.MapGet("/api/v1/endpoint", GetEndpoint);

async Task<IResult> GetEndpoint(INewService service, CancellationToken ct)
{
    var result = await service.GetDataAsync(ct);
    return Results.Ok(result);
}
```

**Caching pattern:**
```csharp
var cacheKey = "tariffs:all";
var cached = await cache.GetAsync<TariffsResponseDto>(cacheKey);

if (cached != null)
    return cached;

var data = await repository.GetAllAsync();
await cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
return data;
```

**Error handling with fallback:**
```csharp
try
{
    var redis = ConnectionMultiplexer.Connect(options);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    builder.Services.AddSingleton<ICacheProvider, RedisCacheProvider>();
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Redis connection failed: {ex.Message}");
    builder.Services.AddSingleton<ICacheProvider, NullCacheProvider>();
}
```

### Database Development

#### DbContext Registration
```csharp
// Program.cs lines 101-107
var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException("PostgreSQL connection string not configured");

builder.Services.AddDbContext<TariffsDbContext>(options =>
{
    options.UseNpgsql(postgresConnection);
});

// Services must be Scoped (not Singleton) when using DbContext
builder.Services.AddScoped<ITariffRepository, TariffRepository>();
builder.Services.AddScoped<ITariffService, TariffService>();
```

#### Migration Workflow

**Creating Migrations:**
```bash
# Navigate to Infrastructure project
cd src/HighPerformanceTariffsAPI.Infrastructure

# Add new migration
dotnet ef migrations add MigrationName --startup-project ../HighPerformanceTariffsAPI.Api

# Review generated migration files in Migrations/
# - YYYYMMDDHHMMSS_MigrationName.cs (Up/Down methods)
# - YYYYMMDDHHMMSS_MigrationName.Designer.cs (metadata)
# - TariffsDbContextModelSnapshot.cs (updated)
```

**Applying Migrations:**
```bash
# Automatic (current setup - runs on startup)
# Program.cs lines 168-170:
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<TariffsDbContext>();
await dbContext.Database.MigrateAsync();

# Manual (recommended for production)
dotnet ef database update --startup-project ../HighPerformanceTariffsAPI.Api

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --startup-project ../HighPerformanceTariffsAPI.Api

# Generate SQL script (for production deployment)
dotnet ef migrations script --startup-project ../HighPerformanceTariffsAPI.Api > migration.sql
```

**Removing Migrations:**
```bash
# Remove last migration (only if not applied to database)
dotnet ef migrations remove --startup-project ../HighPerformanceTariffsAPI.Api
```

#### Seeding Strategy

**Current Implementation (UseAsyncSeeding in OnConfiguring):**
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSeeding((context, _) => { /* sync seeding */ })
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            var tariffsDbContext = (TariffsDbContext)context;
            if (!await tariffsDbContext.Tariffs.AnyAsync(cancellationToken))
            {
                var tariffs = GenerateSeedData();
                tariffsDbContext.Tariffs.AddRange(tariffs);
                await tariffsDbContext.SaveChangesAsync(cancellationToken);
            }
        });
}
```

**Execution Flow:**
1. `MigrateAsync()` applies pending migrations
2. EF Core calls `OnConfiguring` hooks
3. `UseAsyncSeeding` checks if table is empty
4. If empty: generates and inserts 500 records
5. If not empty: skips seeding

**Alternative Patterns (for production):**
- Separate seeding logic (run once via CLI command)
- Use `HasData` in OnModelCreating for static reference data
- Implement custom database initializer
- Use migration-based seeding for versioned data

#### Database Queries

**Query Patterns:**
```csharp
// Read-only queries (use AsNoTracking for performance)
public async Task<IEnumerable<Tariff>> GetAllAsync(int limit, int offset)
{
    return await _context.Tariffs
        .AsNoTracking()           // No change tracking overhead
        .OrderBy(t => t.Id)       // Consistent ordering
        .Skip(offset)             // Pagination
        .Take(limit)              // Limit results
        .ToListAsync();           // Execute async
}

// Single entity query
public async Task<Tariff?> GetByIdAsync(int id)
{
    return await _context.Tariffs
        .AsNoTracking()
        .FirstOrDefaultAsync(t => t.Id == id);
}

// Filtered query with index usage
public async Task<IEnumerable<Tariff>> GetByRegionAsync(string regionCode)
{
    return await _context.Tariffs
        .AsNoTracking()
        .Where(t => t.RegionCode == regionCode)  // Uses IX_Tariffs_RegionCode
        .OrderBy(t => t.Id)
        .ToListAsync();
}
```

#### Connection String Format

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=postgres;Port=5432;Database=tariffs;Username=postgres;Password=postgres"
  }
}
```

**compose.yml (environment override):**
```yaml
environment:
  ConnectionStrings__PostgreSQL: Host=postgres;Port=5432;Database=tariffs;Username=postgres;Password=postgres
```

**Local development (appsettings.Development.json):**
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=tariffs;Username=postgres;Password=postgres"
  }
}
```

### Frontend Development

#### Component Structure
- **Dashboard.svelte:** Main orchestrator (manages notification state)
- **Header.svelte:** Navigation and branding
- **MetricsSection.svelte:** Container for metric cards
- **MetricCard.svelte:** Individual metric display with test button
- **PerformanceChart.svelte:** Performance comparison and load testing
- **ArchitectureInfo.svelte:** Documentation and architecture details
- **NotificationArea.svelte:** Toast notification system

#### Svelte Best Practices
1. **Reactive Declarations:** Use `$:` for computed values
2. **Event Handling:** Use on:click, on:change handlers
3. **Props:** Pass data through component props
4. **Stores (if needed):** Use Svelte stores for global state
5. **Styling:** Use Tailwind classes (no scoped CSS needed due to utility-first)

#### API Integration
```javascript
// Fetch from backend
async function testEndpoint(endpoint) {
  try {
    const response = await fetch(`/api${endpoint}`);
    const data = await response.json();
    return { success: true, data, duration: response.time };
  } catch (error) {
    return { success: false, error: error.message };
  }
}
```

#### Common Patterns

**Reactive state:**
```svelte
<script>
  let loading = false;
  let results = [];

  async function run() {
    loading = true;
    results = await fetch('/api/endpoint').then(r => r.json());
    loading = false;
  }
</script>
```

**Conditional rendering:**
```svelte
{#if loading}
  <p>Loading...</p>
{:else if results.length > 0}
  <div>{results.length} results</div>
{:else}
  <p>No results</p>
{/if}
```

**Event handling:**
```svelte
<button on:click={handleClick}>
  Click me
</button>

<script>
  function handleClick() {
    // Handle click
  }
</script>
```

### Git Workflow

#### Commit Strategy
1. **Atomic Commits:** Each commit represents a single logical change
2. **Descriptive Messages:** Use format: `type: short description`
3. **Types:** feat, fix, docs, refactor, test, chore, infra
4. **Linking:** Reference issue numbers when applicable

#### Commit Examples
```
feat: add caching layer for tariff endpoints
fix: correct Docker build paths to prevent duplicate src/
docs: update README with API endpoint documentation
refactor: extract rate limiting configuration
test: add unit tests for TariffService
chore: update dependencies
infra: update Dockerfile to use full image paths
```

#### Recent Commit History
```
2ec8fe2 fix: update Docker images to use full image paths
5ada4bc fix: remove duplicate Scalar endpoint naming
707de59 fix: move NullCacheProvider class after top-level statements
ec9d4c5 fix: improve Redis connection handling with fallback
1e7fc52 fix: correct Docker COPY --from reference
...
```

## Common Development Tasks

### Running Locally

**Terminal 1: Infrastructure**
```bash
podman-compose up postgres redis
# Wait for health checks to pass
```

**Terminal 2: API**
```bash
dotnet run --project src/HighPerformanceTariffsAPI.Api
# API running at http://localhost:5000
```

**Terminal 3: Frontend**
```bash
cd demo
pnpm dev
# Frontend running at http://localhost:5173
```

### Building & Testing

**Backend:**
```bash
# Build
dotnet build HighPerformanceTariffsAPI.sln

# Run with watch mode
dotnet watch --project src/HighPerformanceTariffsAPI.Api

# Clean build artifacts
dotnet clean
```

**Frontend:**
```bash
cd demo

# Install dependencies
pnpm install

# Development server
pnpm dev

# Production build
pnpm build

# Preview production build
pnpm preview
```

### Docker/Podman

```bash
# Build all images
podman-compose build

# Start all services
podman-compose up -d

# View logs
podman-compose logs -f api

# Stop all services
podman-compose down

# Clean up volumes
podman-compose down -v
```

## Known Issues & Solutions

### Redis Connection Failures
**Issue:** API crashes if Redis is unavailable
**Solution:** NullCacheProvider fallback is automatically used. Check logs for "Warning: Redis connection failed"
**Prevention:** Ensure Redis container is running: `podman-compose ps`

### Port Already in Use
**Issue:** Cannot start services due to port conflicts
**Solution:**
```bash
# Find process using port
lsof -i :5000      # API
lsof -i :5173      # Dev frontend
lsof -i :3000      # Production frontend
lsof -i :6379      # Redis
lsof -i :5432      # PostgreSQL

# Kill process (if safe)
kill -9 <PID>
```

### Podman Image Resolution
**Issue:** "short-name resolution enforced but cannot prompt without a TTY"
**Solution:** Use full image names in compose.yml:
- ✅ `docker.io/library/postgres:16-alpine`
- ❌ `postgres:16-alpine`

### Docker Build Paths
**Issue:** "project file not found" during Docker build
**Solution:** Ensure Dockerfile COPY paths match actual directory structure:
```dockerfile
# ✅ Correct
COPY ["src/HighPerformanceTariffsAPI.Api/HighPerformanceTariffsAPI.Api.csproj", "src/HighPerformanceTariffsAPI.Api/"]

# ❌ Incorrect
COPY ["HighPerformanceTariffsAPI.Api/HighPerformanceTariffsAPI.Api.csproj", "src/HighPerformanceTariffsAPI.Api/"]
```

## AI Assistant Capabilities & Scope

### What AI Assistants CAN Do

**Code Development:**
- Write new classes, methods, and endpoints following Clean Architecture
- Fix bugs and improve existing code
- Refactor code for better clarity and performance
- Create new Svelte components
- Update configuration files
- Write documentation

**Architecture & Design:**
- Suggest improvements to project structure
- Recommend design patterns
- Identify violations of Clean Architecture principles
- Propose caching strategies

**Debugging & Troubleshooting:**
- Analyze error messages and logs
- Identify root causes of failures
- Suggest fixes for common issues
- Help trace through code execution

**DevOps & Infrastructure:**
- Update Dockerfile and compose.yml
- Adjust container configurations
- Implement health checks
- Configure networking

**Documentation:**
- Update README and guides
- Write code comments
- Create API documentation
- Generate troubleshooting guides

### What AI Assistants SHOULD NOT Do

**Without User Approval:**
- Delete or remove functionality
- Make breaking changes to APIs
- Force-push to main branch
- Change security configurations
- Delete databases or volumes
- Modify authentication/authorization without explicit request

**Always Escalate To User:**
- Security vulnerability decisions
- Architecture overhauls
- Database schema changes
- Sensitive configuration changes
- Performance trade-offs requiring business decisions

### Limitations

**Current Constraints:**
- Database migrations run automatically on startup (not ideal for production)
- No user authentication/authorization
- No unit/integration tests implemented
- No CI/CD pipeline
- No load testing framework
- Limited error logging (Console.WriteLine only)

**Future Requirements:**
- Production-ready migration strategy (separate from startup)
- Unit and integration tests
- Structured logging (Serilog)
- Authentication middleware
- Database connection resilience (Polly retry policies)
- Read/write separation (CQRS pattern)
- Advanced performance monitoring
- Load testing tools

## Project Goals

### Completed
✅ Clean Architecture 4-layer implementation
✅ .NET 9 Minimal API with Scalar documentation
✅ Redis distributed caching with fallback pattern
✅ Rate limiting (IP-based, configurable policies)
✅ Svelte 5 + Vite 7 + Tailwind CSS frontend
✅ Docker/Podman multi-service orchestration
✅ Comprehensive documentation
✅ Git version control with atomic commits
✅ Portfolio-grade code quality
✅ PostgreSQL 16 integration with Entity Framework Core 9.0
✅ Automatic database migrations on startup
✅ Database seeding with 500 realistic tariff records
✅ EF Core repository pattern with AsNoTracking optimization

### In Progress
⏳ Unit test coverage
⏳ Integration tests

### Future Enhancements
⏳ Production-ready migration strategy (separate from startup)
⏳ Database connection resilience (Polly retry policies)
⏳ Read/write separation (CQRS pattern)
⏳ CI/CD pipeline (GitHub Actions)
⏳ Structured logging (Serilog)
⏳ Advanced monitoring and telemetry
⏳ API versioning strategy
⏳ Load testing framework
⏳ Kubernetes deployment manifests

## Support & Escalation

### When to Ask the User

1. **Major architectural changes** - Ask before restructuring projects
2. **Breaking API changes** - Confirm before changing endpoint signatures
3. **New technologies** - Ask before adding new dependencies or frameworks
4. **Database schema** - Ask before making PostgreSQL decisions
5. **Security decisions** - Always ask about security implications
6. **Business logic** - Ask when unsure about expected behavior
7. **Performance trade-offs** - Ask when there are multiple valid approaches

### How to Ask
- Use clear, specific questions
- Provide context and options
- Explain the implications of each choice
- Ask for explicit confirmation before proceeding

---

**Last Updated:** November 5, 2025
**Status:** Production Ready
**License:** MIT
