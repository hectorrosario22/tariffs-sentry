# Quick Start Guide - High Performance Tariffs API

## Prerequisites

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** and **pnpm** - `npm install -g pnpm`
- **Podman** (or Docker) - [Download](https://podman.io/)
- **Git** - For version control

## Setup (First Time Only)

### 1. Clone and Initialize
```bash
cd tariffs-sentry
git status  # Verify git repo is initialized
```

### 2. Configure Environment
```bash
cp .env.example .env
# No changes needed for local development
```

### 3. Build Backend (Verify Compilation)
```bash
dotnet build HighPerformanceTariffsAPI.sln
```

### 4. Build Frontend (Verify Compilation)
```bash
cd demo
pnpm install
pnpm build
cd ..
```

## Development Setup

### Option 1: Local Development (Recommended for Development)

**Terminal 1 - Redis & Database**
```bash
podman-compose up postgres redis
# Wait for both services to be healthy
```

**Terminal 2 - .NET API**
```bash
dotnet run --project src/HighPerformanceTariffsAPI.Api
# API will be available at http://localhost:5000
```

**Terminal 3 - Svelte Frontend**
```bash
cd demo
pnpm dev
# Frontend will be available at http://localhost:5173
```

**Terminal 4 - Open in Browser**
```bash
# Frontend: http://localhost:5173
# API Docs: http://localhost:5000
```

### Option 2: Full Containerized Stack (Recommended for Demos)

```bash
# Start everything
podman-compose up -d

# Check status
podman-compose ps

# View logs
podman-compose logs -f api

# Access services
# Frontend: http://localhost:3000
# API: http://localhost:5000
# API Docs: http://localhost:5000
```

## Verification Checklist

- [ ] Backend builds without errors: `dotnet build`
- [ ] Frontend builds without errors: `cd demo && pnpm build`
- [ ] Can access API docs: http://localhost:5000
- [ ] Can access frontend: http://localhost:3000 or http://localhost:5173
- [ ] Test slow endpoint: Click "Test Endpoint" on "Slow Endpoint" card
- [ ] Test fast endpoint: Click "Test Endpoint" on "Fast Endpoint" card

## Common Commands

### Backend
```bash
# Build
dotnet build HighPerformanceTariffsAPI.sln

# Run API locally
dotnet run --project src/HighPerformanceTariffsAPI.Api

# Watch mode (auto-restart on changes)
dotnet watch --project src/HighPerformanceTariffsAPI.Api

# View dependencies
dotnet list HighPerformanceTariffsAPI.sln
```

### Frontend
```bash
# Navigate to demo folder
cd demo

# Install dependencies
pnpm install

# Start dev server
pnpm dev

# Build for production
pnpm build

# Preview production build
pnpm preview
```

### Docker/Podman
```bash
# Start specific services
podman-compose up -d api
podman-compose up -d redis

# Stop all services
podman-compose down

# Clean everything (including volumes)
podman-compose down -v

# View service logs
podman-compose logs api     # API logs only
podman-compose logs -f      # All logs with follow
podman-compose logs --tail=50 redis  # Last 50 lines of redis

# Rebuild images
podman-compose build

# Push to registry
podman-compose push
```

## Project Structure Quick Reference

```
tariffs-sentry/
├── src/                    # .NET projects
│   ├── Domain/            # Business entities
│   ├── Application/       # DTOs & services
│   ├── Infrastructure/    # Redis, data access
│   └── Api/               # Minimal API endpoints
├── demo/                  # Svelte frontend
├── compose.yml            # Docker configuration
└── README.md             # Full documentation
```

## API Testing

### Using curl
```bash
# Test slow endpoint
curl http://localhost:5000/api/v1/tariffs/slow

# Test fast endpoint
curl http://localhost:5000/api/v1/tariffs/fast

# Check health
curl http://localhost:5000/health

# View API docs
open http://localhost:5000
```

### Using the Frontend
1. Go to http://localhost:5173 (or 3000 if containerized)
2. Click "Test Endpoint" buttons on metric cards
3. Run "Performance Test" to compare slow vs fast

## Troubleshooting

### Backend won't compile
```bash
# Clean build artifacts
dotnet clean
dotnet build

# Restore packages
dotnet restore

# Check .NET version
dotnet --version  # Should be 9.0.x
```

### Frontend won't build
```bash
# Clean node modules
rm -rf demo/node_modules demo/pnpm-lock.yaml
cd demo
pnpm install
pnpm build
```

### Redis connection error
```bash
# Check if redis is running
podman ps | grep redis

# Check connection string
# Should be: redis:6379 (containerized) or localhost:6379 (local)

# Test redis connection
redis-cli ping
```

### Port already in use
```bash
# Find process using port
lsof -i :5000      # API port
lsof -i :3000      # Frontend port
lsof -i :5173      # Dev server port
lsof -i :6379      # Redis port

# Kill process (if safe)
kill -9 <PID>
```

## Performance Testing

### Load Test (Using Frontend)
1. Navigate to frontend
2. Click "Run Performance Test"
3. Observe metrics for 5 test iterations
4. View improvement percentage

### Manual Load Test
```bash
# Slow endpoint (5 requests)
for i in {1..5}; do
  time curl http://localhost:5000/api/v1/tariffs/slow > /dev/null
done

# Fast endpoint (5 requests)
for i in {1..5}; do
  time curl http://localhost:5000/api/v1/tariffs/fast > /dev/null
done
```

## Next Steps

1. **Learn the Architecture** - Read [README.md](README.md)
2. **Explore Code** - Check [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)
3. **Add Features** - See "Future Improvements" in README
4. **Deploy** - Use `podman-compose` with proper configuration

## Resources

- [.NET 9 Docs](https://learn.microsoft.com/dotnet/)
- [Svelte Docs](https://svelte.dev/docs)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [Podman Documentation](https://podman.io/)
- [Redis Documentation](https://redis.io/docs/)

## Support

For issues or questions:
1. Check the comprehensive [README.md](README.md)
2. Review [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)
3. Check logs: `podman-compose logs -f <service>`
4. Verify all prerequisites are installed

---

**Happy coding! 🚀**
