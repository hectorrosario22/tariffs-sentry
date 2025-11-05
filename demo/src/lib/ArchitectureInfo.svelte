<script>
  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';

  function openDocumentation() {
    window.open(`${apiUrl}`, '_blank');
  }
</script>

<div class="card">
  <h2 class="text-2xl font-bold text-slate-100 mb-6">Architecture Overview</h2>

  <div class="grid grid-cols-1 md:grid-cols-2 gap-8 mb-6">
    <!-- Backend Architecture -->
    <div>
      <h3 class="text-lg font-semibold text-slate-100 mb-4 flex items-center gap-2">
        <span>🏗️</span> Backend Architecture
      </h3>
      <div class="p-4 rounded-lg border-2 border-orange-500 space-y-2 text-sm text-slate-300" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 12px rgba(255, 107, 53, 0.15);">
        <p><strong>Framework:</strong> .NET 9 with Minimal APIs</p>
        <p><strong>Language:</strong> C#</p>
        <p><strong>Database:</strong> PostgreSQL 16</p>
        <p><strong>Cache:</strong> Redis 7 (Distributed)</p>
        <p><strong>Documentation:</strong> Scalar OpenAPI UI</p>
      </div>
    </div>

    <!-- Frontend Architecture -->
    <div>
      <h3 class="text-lg font-semibold text-slate-100 mb-4 flex items-center gap-2">
        <span>⚡</span> Frontend Architecture
      </h3>
      <div class="p-4 rounded-lg border-2 border-blue-400 space-y-2 text-sm text-slate-300" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 12px rgba(59, 130, 246, 0.15);">
        <p><strong>Framework:</strong> Svelte 5</p>
        <p><strong>Build Tool:</strong> Vite 7</p>
        <p><strong>Styling:</strong> Tailwind CSS</p>
        <p><strong>Package Manager:</strong> pnpm</p>
        <p><strong>Runtime:</strong> Node.js 18+</p>
      </div>
    </div>
  </div>

  <!-- Key Features -->
  <div class="mb-6">
    <h3 class="text-lg font-semibold text-slate-100 mb-4 flex items-center gap-2">
      <span>✨</span> Key Features
    </h3>
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="p-4 rounded-lg border border-orange-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(255, 107, 53, 0.1);">
        <p class="font-semibold text-orange-400 mb-2">🚀 High Performance</p>
        <p class="text-sm text-slate-400">Distributed caching with Redis reduces latency by 97%</p>
      </div>
      <div class="p-4 rounded-lg border border-blue-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(59, 130, 246, 0.1);">
        <p class="font-semibold text-blue-400 mb-2">🏛️ Clean Architecture</p>
        <p class="text-sm text-slate-400">Layered design following SOLID principles</p>
      </div>
      <div class="p-4 rounded-lg border border-green-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(16, 185, 129, 0.1);">
        <p class="font-semibold text-green-400 mb-2">🛡️ Rate Limiting</p>
        <p class="text-sm text-slate-400">Middleware protection against abuse</p>
      </div>
    </div>
  </div>

  <!-- Project Structure -->
  <div class="mb-6">
    <h3 class="text-lg font-semibold text-slate-100 mb-4 flex items-center gap-2">
      <span>📁</span> Project Structure
    </h3>
    <div class="p-4 rounded-lg font-mono text-xs text-slate-300 overflow-x-auto" style="background: rgba(31, 41, 55, 0.6); border: 1px solid rgba(148, 163, 184, 0.2);">
      <div class="space-y-1">
        <div>tariffs-sentry/</div>
        <div class="ml-4">├── src/</div>
        <div class="ml-8">├── Domain/          (Entities, Interfaces)</div>
        <div class="ml-8">├── Application/     (Business Logic, DTOs)</div>
        <div class="ml-8">├── Infrastructure/  (Redis, Repositories)</div>
        <div class="ml-8">└── Api/             (Minimal API, Endpoints)</div>
        <div class="ml-4">├── demo/            (Svelte Frontend)</div>
        <div class="ml-4">├── compose.yml      (Docker Compose)</div>
        <div class="ml-4">└── README.md        (Documentation)</div>
      </div>
    </div>
  </div>

  <!-- API Endpoints -->
  <div class="mb-6">
    <h3 class="text-lg font-semibold text-slate-100 mb-4 flex items-center gap-2">
      <span>🔌</span> API Endpoints (v1)
    </h3>
    <div class="space-y-2 text-sm">
      <div class="flex justify-between items-start p-3 rounded border-2 border-orange-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(255, 107, 53, 0.1);">
        <div>
          <p class="font-semibold text-orange-400">GET /api/v1/tariffs/slow</p>
          <p class="text-slate-400">Direct database read (~300ms)</p>
        </div>
      </div>
      <div class="flex justify-between items-start p-3 rounded border-2 border-green-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(16, 185, 129, 0.1);">
        <div>
          <p class="font-semibold text-green-400">GET /api/v1/tariffs/fast</p>
          <p class="text-slate-400">Redis cached endpoint (&lt;10ms)</p>
        </div>
      </div>
      <div class="flex justify-between items-start p-3 rounded border-2 border-blue-400" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(59, 130, 246, 0.1);">
        <div>
          <p class="font-semibold text-blue-400">GET /</p>
          <p class="text-slate-400">Scalar API documentation</p>
        </div>
      </div>
    </div>
  </div>

  <!-- Technology Stack -->
  <div class="p-4 rounded-lg border-2 border-orange-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 12px rgba(255, 107, 53, 0.15);">
    <h3 class="font-semibold text-slate-100 mb-3">💡 Technology Stack</h3>
    <p class="text-sm text-slate-400 mb-4">
      This project demonstrates modern development practices: Clean Architecture, distributed caching, API documentation,
      containerization with Docker/Podman, and responsive frontend development with Svelte.
    </p>
    <button
      on:click={openDocumentation}
      class="btn-primary w-full md:w-auto"
    >
      📚 View Full API Documentation
    </button>
  </div>
</div>

<style></style>
