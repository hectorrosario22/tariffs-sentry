# High Performance Tariffs API - Demo Frontend

A modern, responsive Svelte 5 + Vite 7 + Tailwind CSS frontend application demonstrating high-performance API patterns and distributed caching strategies.

## Overview

This demo frontend showcases the performance difference between:
- **Direct Database Access** - Traditional approach with simulated 300ms latency
- **Distributed Caching** - Redis-backed caching with <10ms response times

The application provides interactive testing, performance metrics, and architectural documentation all in one dashboard.

## Technology Stack

- **Svelte 5.43.3** - Reactive, compiler-driven UI framework
- **Vite 7.2.0** - Lightning-fast build tool and dev server
- **Tailwind CSS 3.4.3** - Utility-first CSS framework
- **pnpm** - Fast, space-efficient package manager
- **Node.js 18+** - JavaScript runtime

## Features

### 📊 Metrics Dashboard
- **Direct Database Access:** Test latency of direct calls (~300ms)
- **Cached Access:** Test latency of Redis-cached calls (<10ms)
- **Real-time Results:** Immediate feedback with response times and data counts

### 🚀 Performance Testing
- **Load Test Runner:** Run 5 iterations comparing slow vs. fast endpoints
- **Performance Comparison:** View average response times and improvement percentages
- **Visual Feedback:** Real-time progress updates and results display

### 🏗️ Architecture Documentation
- **System Overview:** Backend and frontend architecture diagrams
- **Component Structure:** Details on how the frontend is organized
- **Project Information:** Quick reference to project structure and API endpoints

### 🎨 User Experience
- **Responsive Design:** Works on desktop, tablet, and mobile
- **Toast Notifications:** Real-time feedback for API operations
- **Clean Interface:** Modern, professional UI with gradient accents

## Component Architecture

```
App.svelte
└── Dashboard.svelte (main orchestrator)
    ├── Header.svelte (navigation)
    ├── MetricsSection.svelte
    │   ├── MetricCard.svelte (slow endpoint)
    │   ├── MetricCard.svelte (fast endpoint)
    │   └── MetricCard.svelte (statistics)
    ├── PerformanceChart.svelte (testing & comparison)
    ├── ArchitectureInfo.svelte (documentation)
    └── NotificationArea.svelte (toast messages)
```

### Component Descriptions

**Dashboard.svelte**
- Main layout component
- Manages notification state
- Orchestrates all sections

**Header.svelte**
- Application title and description
- Link to API documentation

**MetricsSection.svelte**
- Container for metric cards
- Displays three key metrics:
  1. Direct Database Access
  2. Cached Access
  3. Performance Statistics

**MetricCard.svelte**
- Reusable metric display card
- Shows endpoint name, response time, data count
- "Test Endpoint" button for manual testing

**PerformanceChart.svelte**
- Performance load testing interface
- Runs 5 iterations of both endpoints
- Calculates averages and improvement percentages
- Visual progress indicators

**ArchitectureInfo.svelte**
- Backend architecture overview
- Frontend project structure
- API endpoints reference
- Key features list

**NotificationArea.svelte**
- Toast notification system
- Supports success, danger, and info messages
- Auto-dismiss capability
- Positioned in top-right corner

## Setup & Development

### Prerequisites
- **Node.js 18+** - [Download](https://nodejs.org/)
- **pnpm** - Install via: `npm install -g pnpm`

### Installation

```bash
# Navigate to demo directory
cd demo

# Install dependencies
pnpm install
```

### Development Server

```bash
pnpm dev
```

The frontend will start at `http://localhost:5173` with hot module replacement (HMR) enabled.

**Note:** The dev server proxies `/api` requests to `http://localhost:5000` (the backend API). Make sure the backend is running.

### Production Build

```bash
# Build for production
pnpm build

# Preview production build locally
pnpm preview
```

The production build generates optimized files in the `dist/` directory.

## Configuration

### Environment Variables

Create a `.env` file (or `.env.local` for local overrides):

```
VITE_API_URL=http://localhost:5000
```

**Available Variables:**
- `VITE_API_URL` - Backend API base URL (default: `http://localhost:5000`)

### Vite Configuration

Key settings in `vite.config.js`:

```javascript
export default defineConfig({
  plugins: [svelte()],
  server: {
    port: 5173,                    // Dev server port
    host: true,                    // Listen on all interfaces
    proxy: {
      '/api': {
        target: 'http://localhost:5000',  // Backend URL
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: 'dist',               // Production output directory
    sourcemap: false,             // No source maps in production
  },
});
```

## API Integration

### Endpoints Used

**Health Check:**
```javascript
GET /health
Response: { status: "healthy", timestamp: "2025-11-05T10:30:00Z" }
```

**Slow Endpoint:**
```javascript
GET /api/v1/tariffs/slow
Response: {
  data: [{ id, regionCode, rate, effectiveDate }, ...],
  total: 500,
  timestamp: "2025-11-05T10:30:00Z",
  fromCache: false,
  cachedAt: null
}
Duration: ~300ms
```

**Fast Endpoint:**
```javascript
GET /api/v1/tariffs/fast
Response: {
  data: [...],
  total: 500,
  timestamp: "2025-11-05T10:30:00Z",
  fromCache: true,
  cachedAt: "2025-11-05T10:25:00Z"
}
Duration: <10ms
```

### Making API Calls

```javascript
// Fetch from API endpoint
async function testEndpoint(endpoint) {
  try {
    const startTime = performance.now();
    const response = await fetch(`/api${endpoint}`);
    const data = await response.json();
    const duration = performance.now() - startTime;

    return {
      success: true,
      data,
      duration: Math.round(duration)
    };
  } catch (error) {
    return {
      success: false,
      error: error.message
    };
  }
}
```

## Styling with Tailwind CSS

### Global Styles (`src/app.css`)

```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

### Common Tailwind Classes Used

- **Layout:** `grid`, `flex`, `gap-4`, `p-6`, `m-4`
- **Colors:** `bg-blue-500`, `text-gray-900`, `text-white`
- **Typography:** `text-xl`, `font-bold`, `text-center`
- **Effects:** `rounded-lg`, `shadow-lg`, `hover:shadow-xl`
- **Responsive:** `sm:`, `md:`, `lg:` breakpoints
- **Gradients:** `bg-gradient-to-r`, `from-blue-500`, `to-purple-600`

### Custom Utility Classes

Additional utilities defined in `app.css`:

```css
/* Gradient card background */
.card-gradient {
  background: linear-gradient(...);
}
```

## Directory Structure

```
demo/
├── src/                          # Source code
│   ├── App.svelte               # Root component
│   ├── main.js                  # Entry point
│   ├── app.css                  # Global styles (Tailwind)
│   └── lib/                     # Reusable components
│       ├── Dashboard.svelte
│       ├── Header.svelte
│       ├── MetricsSection.svelte
│       ├── MetricCard.svelte
│       ├── PerformanceChart.svelte
│       ├── ArchitectureInfo.svelte
│       └── NotificationArea.svelte
├── public/                       # Static assets
│   └── vite.svg
├── dist/                         # Production build output
├── node_modules/                # Dependencies
├── package.json                 # Dependencies & scripts
├── pnpm-lock.yaml              # Locked dependency versions
├── vite.config.js              # Vite configuration
├── svelte.config.js            # Svelte configuration
├── tailwind.config.js          # Tailwind configuration
├── postcss.config.js           # PostCSS configuration
├── jsconfig.json               # JavaScript configuration
├── index.html                  # HTML entry point
├── Dockerfile                  # Docker image definition
├── .dockerignore               # Docker build ignore
├── .env.example                # Environment template
└── README.md                   # This file
```

## Docker Deployment

### Building the Image

```bash
# Build from project root
podman-compose build demo

# Or manually
podman build -t tariffs-demo ./demo
```

### Running in Container

```bash
# Via compose
podman-compose up demo

# Manual container
podman run -p 3000:80 tariffs-demo
```

The frontend will be served at `http://localhost:3000`

### Dockerfile Details

```dockerfile
FROM node:18-alpine AS build
WORKDIR /app
COPY package.json pnpm-lock.yaml* ./
RUN npm install -g pnpm && pnpm install --frozen-lockfile
COPY . .
RUN pnpm build

FROM node:18-alpine
WORKDIR /app
RUN npm install -g http-server
COPY --from=build /app/dist ./dist
EXPOSE 80
CMD ["http-server", "dist", "-p", "80", "-c-1"]
```

**Multi-stage build benefits:**
- **Build stage:** Includes all build tools (Vite, dependencies)
- **Runtime stage:** Only includes HTTP server and built files
- **Smaller image:** Final image excludes build tools

## Common Commands

```bash
# Development
pnpm dev              # Start dev server with HMR
pnpm build            # Production build
pnpm preview          # Preview production build

# Package management
pnpm install          # Install dependencies
pnpm add <package>    # Add dependency
pnpm remove <package> # Remove dependency
pnpm update           # Update all dependencies

# Linting & Formatting (if configured)
pnpm lint             # Check code quality
pnpm format           # Format code
```

## Troubleshooting

### Dev Server Issues

**Port 5173 already in use:**
```bash
# Find process
lsof -i :5173

# Kill process (if safe)
kill -9 <PID>

# Or specify different port
pnpm dev -- --port 5174
```

**Hot Module Replacement (HMR) not working:**
- Check that `host: true` is set in `vite.config.js`
- Clear browser cache (Cmd+Shift+R or Ctrl+Shift+R)
- Restart dev server

**API requests failing:**
- Ensure backend API is running on `http://localhost:5000`
- Check proxy configuration in `vite.config.js`
- Verify CORS is enabled on backend (`AllowAll` policy)

### Build Issues

**Build fails with "module not found":**
```bash
# Clean install
rm -rf node_modules pnpm-lock.yaml
pnpm install
pnpm build
```

**Build slower than expected:**
- Check for large dependencies: `npm list --depth=0`
- Consider code splitting with dynamic imports
- Use production environment: `NODE_ENV=production pnpm build`

### Component Issues

**Svelte component not updating:**
- Use reactive declarations: `$:` for computed values
- Use bind for two-way binding: `bind:value`
- Check for missing event handlers

**Styling not applying:**
- Ensure Tailwind classes are used (not custom CSS)
- Check `tailwind.config.js` includes correct paths
- Clear Tailwind cache: `rm -rf .tailwindcss-cache`

## Performance Tips

### Build Optimization
- Use `pnpm` instead of npm (30% faster)
- Enable code splitting for large components
- Use dynamic imports: `import('Component')`
- Remove unused dependencies regularly

### Runtime Optimization
- Use `:key` in `{#each}` loops for performance
- Debounce frequently-called functions
- Use `lazy` loading for off-screen components
- Minimize re-renders with proper reactivity

### Bundle Size
- Tree-shake unused Tailwind utilities
- Minimize external dependencies
- Use Lighthouse DevTools to profile

## Browser Support

Svelte 5 and Vite 7 target modern browsers:
- Chrome/Edge 120+
- Firefox 119+
- Safari 17+
- Mobile browsers (iOS 17+, Android Chrome 120+)

## Resources

- [Svelte Documentation](https://svelte.dev/docs)
- [Vite Documentation](https://vitejs.dev/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [MDN Web Docs](https://developer.mozilla.org/)

## Related Documentation

- **Main Project:** [README.md](../README.md)
- **Backend Architecture:** [AGENTS.md](../AGENTS.md)
- **Project Structure:** [PROJECT_STRUCTURE.md](../PROJECT_STRUCTURE.md)
- **Quick Start:** [QUICKSTART.md](../QUICKSTART.md)

## License

MIT - See [LICENSE](../LICENSE) file

---

**Last Updated:** November 5, 2025
**Status:** Production Ready
