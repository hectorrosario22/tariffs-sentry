<script>
  let slowData = [];
  let fastData = [];
  let avgSlow = 0;
  let avgFast = 0;

  async function runPerformanceTest() {
    slowData = [];
    fastData = [];

    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
    const iterations = 5;

    for (let i = 0; i < iterations; i++) {
      // Test slow endpoint
      try {
        const start = performance.now();
        await fetch(`${apiUrl}/api/v1/tariffs/slow`);
        const time = performance.now() - start;
        slowData = [...slowData, { time: time.toFixed(0), label: `Run ${i + 1}` }];
      } catch (e) {
        slowData = [...slowData, { time: 'Error', label: `Run ${i + 1}` }];
      }

      // Test fast endpoint
      try {
        const start = performance.now();
        await fetch(`${apiUrl}/api/v1/tariffs/fast`);
        const time = performance.now() - start;
        fastData = [...fastData, { time: time.toFixed(0), label: `Run ${i + 1}` }];
      } catch (e) {
        fastData = [...fastData, { time: 'Error', label: `Run ${i + 1}` }];
      }

      // Add delay between tests
      await new Promise(resolve => setTimeout(resolve, 500));
    }

    // Calculate averages
    const slowTimes = slowData.map(d => parseFloat(d.time)).filter(t => !isNaN(t));
    const fastTimes = fastData.map(d => parseFloat(d.time)).filter(t => !isNaN(t));

    avgSlow = slowTimes.length > 0 ? (slowTimes.reduce((a, b) => a + b) / slowTimes.length).toFixed(0) : 0;
    avgFast = fastTimes.length > 0 ? (fastTimes.reduce((a, b) => a + b) / fastTimes.length).toFixed(0) : 0;
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

  {#if slowData.length > 0}
    <div class="grid grid-cols-2 gap-6 mt-6">
      <!-- Slow Endpoint Results -->
      <div class="p-4 rounded-lg border-2 border-orange-500" style="background: rgba(31, 41, 55, 0.8); box-shadow: 0 0 8px rgba(255, 107, 53, 0.1);">
        <h3 class="font-semibold text-orange-400 mb-3">Slow Endpoint (Database)</h3>
        <div class="space-y-2">
          {#each slowData as result}
            <div class="flex justify-between text-sm">
              <span class="text-slate-400">{result.label}</span>
              <span class="font-bold text-orange-400">{result.time}ms</span>
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
            <div class="flex justify-between text-sm">
              <span class="text-slate-400">{result.label}</span>
              <span class="font-bold text-green-400">{result.time}ms</span>
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

<style></style>
