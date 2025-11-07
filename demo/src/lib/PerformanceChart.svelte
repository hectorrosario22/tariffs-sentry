<script lang="ts">
  import RequestDetailModal from './RequestDetailModal.svelte';

  interface TestResult {
    time: string | number;
    label: string;
    status?: number;
    url?: string;
    requestHeaders?: Record<string, string>;
    responseHeaders?: Record<string, string>;
    responseBody?: any;
  }

  let slowData: TestResult[] = $state([]);
  let fastData: TestResult[] = $state([]);
  let avgSlow = $state(0);
  let avgFast = $state(0);
  let bypassKey = $state('');
  let isModalOpen = $state(false);
  let selectedRequest: any = $state(null);

  // Get bypass key from environment variable
  const validBypassKey = import.meta.env.VITE_BYPASS_KEY || 'performance-test-bypass';

  function fillBypassKey() {
    bypassKey = validBypassKey;
  }

  function openDetailsModal(result: TestResult, endpoint: string) {
    selectedRequest = {
      url: result.url || '',
      method: 'GET',
      requestHeaders: result.requestHeaders || {},
      responseHeaders: result.responseHeaders || {},
      responseBody: result.responseBody || {},
      status: result.status || 0,
      latency: typeof result.time === 'number' ? result.time : parseInt(result.time)
    };
    isModalOpen = true;
  }

  function closeModal() {
    isModalOpen = false;
    selectedRequest = null;
  }

  async function runPerformanceTest() {
    slowData = [];
    fastData = [];

    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
    const iterations = 5;

    const headers: Record<string, string> = {};
    if (bypassKey.trim()) {
      headers['X-Bypass-RateLimit'] = bypassKey;
    }

    for (let i = 0; i < iterations; i++) {
      // Test slow endpoint
      try {
        const url = `${apiUrl}/api/v1/tariffs/slow`;
        const start = performance.now();
        const response = await fetch(url, { headers });
        const time = performance.now() - start;

        // Capturar headers de respuesta
        const responseHeaders: Record<string, string> = {};
        response.headers.forEach((value, key) => {
          responseHeaders[key] = value;
        });

        // Capturar body de respuesta
        const responseBody = await response.json();

        slowData = [...slowData, {
          time: time.toFixed(0),
          label: `Run ${i + 1}`,
          status: response.status,
          url,
          requestHeaders: headers,
          responseHeaders,
          responseBody
        }];
      } catch (e) {
        slowData = [...slowData, {
          time: 'Error',
          label: `Run ${i + 1}`,
          status: 0
        }];
      }

      // Test fast endpoint
      try {
        const url = `${apiUrl}/api/v1/tariffs/fast`;
        const start = performance.now();
        const response = await fetch(url, { headers });
        const time = performance.now() - start;

        // Capturar headers de respuesta
        const responseHeaders: Record<string, string> = {};
        response.headers.forEach((value, key) => {
          responseHeaders[key] = value;
        });

        // Capturar body de respuesta
        const responseBody = await response.json();

        fastData = [...fastData, {
          time: time.toFixed(0),
          label: `Run ${i + 1}`,
          status: response.status,
          url,
          requestHeaders: headers,
          responseHeaders,
          responseBody
        }];
      } catch (e) {
        fastData = [...fastData, {
          time: 'Error',
          label: `Run ${i + 1}`,
          status: 0
        }];
      }

      // Add delay between tests
      await new Promise(resolve => setTimeout(resolve, 500));
    }

    // Calculate averages
    const slowTimes = slowData.map(d => parseFloat(String(d.time))).filter(t => !isNaN(t));
    const fastTimes = fastData.map(d => parseFloat(String(d.time))).filter(t => !isNaN(t));

    avgSlow = slowTimes.length > 0 ? Number((slowTimes.reduce((a, b) => a + b) / slowTimes.length).toFixed(0)) : 0;
    avgFast = fastTimes.length > 0 ? Number((fastTimes.reduce((a, b) => a + b) / fastTimes.length).toFixed(0)) : 0;
  }
</script>

<div class="card">
  <div class="flex justify-between items-center mb-6">
    <div>
      <h2 class="text-2xl font-bold text-slate-100">Performance Comparison</h2>
      <p class="text-slate-400 text-sm mt-1">Run 5 tests to compare slow vs fast endpoints</p>
    </div>
    <button on:click={runPerformanceTest} class="btn-primary">
      🚀 Run Performance Test
    </button>
  </div>

  <!-- Bypass Key Section -->
  <div class="mt-6 p-4 rounded-lg border border-slate-700" style="background: rgba(15, 23, 42, 0.5);">
    <div class="flex flex-col gap-3">
      <div>
        <label for="bypassKey" class="text-sm font-semibold text-slate-300 block mb-2">
          Bypass Key (Optional)
        </label>
        <p class="text-xs text-slate-400 mb-3">
          Use the button to autofill the correct key: <span class="font-mono text-blue-400">{validBypassKey}</span>
        </p>
        <div class="flex gap-2">
          <input
            id="bypassKey"
            type="text"
            bind:value={bypassKey}
            placeholder="Enter bypass key..."
            class="flex-1 px-3 py-2 rounded-md bg-slate-800 border border-slate-600 text-slate-100 placeholder-slate-500 focus:outline-none focus:border-blue-500"
          />
          <button
            on:click={fillBypassKey}
            class="px-4 py-2 rounded-md font-semibold text-white transition-all"
            style="background: linear-gradient(135deg, #22c55e, #16a34a); box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);"
          >
            Fill Key
          </button>
        </div>
      </div>
      <div class="text-xs text-slate-400 pt-2 border-t border-slate-700">
        {#if bypassKey.trim() === validBypassKey}
          <span class="text-green-400">✓ Valid key entered - tests will run without rate limiting</span>
        {:else if bypassKey.trim()}
          <span class="text-orange-400">⚠ Invalid key - tests will respect rate limiting</span>
        {:else}
          <span class="text-slate-400">No key - tests will respect rate limiting</span>
        {/if}
      </div>
    </div>
  </div>

  {#if slowData.length > 0}
    <div class="grid grid-cols-2 gap-6 mt-6">
      <!-- Slow Endpoint Results -->
      <div class="p-4 rounded-lg border-2 border-orange-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(255, 107, 53, 0.1);">
        <h3 class="font-semibold text-orange-400 mb-3">Slow Endpoint (Database)</h3>
        <div class="space-y-2">
          {#each slowData as result}
            <div class="flex justify-between items-center text-sm p-2 rounded" style="background: rgba(0, 0, 0, 0.2);">
              <div class="flex items-center gap-2">
                <span class="text-slate-400">{result.label}</span>
                {#if result.status}
                  <span class="status-badge-small" class:success={result.status === 200} class:error={result.status >= 400}>
                    {result.status}
                  </span>
                {/if}
              </div>
              <div class="flex items-center gap-2">
                <span class="font-bold text-orange-400">{result.time}ms</span>
                {#if result.status}
                  <button
                    class="details-button"
                    on:click={() => openDetailsModal(result, 'slow')}
                    title="View details"
                  >
                    🔍
                  </button>
                {/if}
              </div>
            </div>
          {/each}
        </div>
        <div class="mt-4 pt-4 border-t border-orange-500/30">
          <p class="text-sm text-slate-400">Average Response Time</p>
          <p class="text-2xl font-bold text-orange-400">{avgSlow}ms</p>
        </div>
      </div>

      <!-- Fast Endpoint Results -->
      <div class="p-4 rounded-lg border-2 border-green-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(16, 185, 129, 0.1);">
        <h3 class="font-semibold text-green-400 mb-3">Fast Endpoint (Cached)</h3>
        <div class="space-y-2">
          {#each fastData as result}
            <div class="flex justify-between items-center text-sm p-2 rounded" style="background: rgba(0, 0, 0, 0.2);">
              <div class="flex items-center gap-2">
                <span class="text-slate-400">{result.label}</span>
                {#if result.status}
                  <span class="status-badge-small" class:success={result.status === 200} class:error={result.status >= 400}>
                    {result.status}
                  </span>
                {/if}
              </div>
              <div class="flex items-center gap-2">
                <span class="font-bold text-green-400">{result.time}ms</span>
                {#if result.status}
                  <button
                    class="details-button"
                    on:click={() => openDetailsModal(result, 'fast')}
                    title="View details"
                  >
                    🔍
                  </button>
                {/if}
              </div>
            </div>
          {/each}
        </div>
        <div class="mt-4 pt-4 border-t border-green-500/30">
          <p class="text-sm text-slate-400">Average Response Time</p>
          <p class="text-2xl font-bold text-green-400">{avgFast}ms</p>
        </div>
      </div>
    </div>

    {#if avgSlow > 0 && avgFast > 0}
      <div class="mt-6 p-4 border-2 rounded-lg border-orange-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 12px rgba(255, 107, 53, 0.15);">
        <p class="text-sm text-slate-400 mb-2">Performance Improvement</p>
        <div class="flex items-end gap-4">
          <div>
            <p class="text-3xl font-bold text-transparent" style="background: linear-gradient(135deg, #FF6B35, #60A5FA); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;">
              {((1 - avgFast / avgSlow) * 100).toFixed(1)}%
            </p>
            <p class="text-sm text-slate-400">faster</p>
          </div>
          <div class="text-sm text-slate-400">
            <p>Slow: {avgSlow}ms → Fast: {avgFast}ms</p>
          </div>
        </div>
      </div>
    {/if}
  {/if}
</div>

<!-- Modal de detalles -->
<RequestDetailModal
  isOpen={isModalOpen}
  onClose={closeModal}
  requestDetails={selectedRequest}
/>

<style>
  .status-badge-small {
    display: inline-block;
    padding: 0.125rem 0.5rem;
    border-radius: 0.25rem;
    font-size: 0.75rem;
    font-weight: 600;
    font-family: 'Courier New', monospace;
  }

  .status-badge-small.success {
    background: rgba(34, 197, 94, 0.2);
    color: #22c55e;
    border: 1px solid rgba(34, 197, 94, 0.5);
  }

  .status-badge-small.error {
    background: rgba(239, 68, 68, 0.2);
    color: #ef4444;
    border: 1px solid rgba(239, 68, 68, 0.5);
  }

  .details-button {
    background: rgba(59, 130, 246, 0.2);
    border: 1px solid rgba(59, 130, 246, 0.4);
    color: #60a5fa;
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem;
    cursor: pointer;
    font-size: 0.875rem;
    transition: all 0.2s ease;
  }

  .details-button:hover {
    background: rgba(59, 130, 246, 0.3);
    border-color: rgba(59, 130, 246, 0.6);
    transform: scale(1.1);
  }

  .details-button:active {
    transform: scale(0.95);
  }
</style>
