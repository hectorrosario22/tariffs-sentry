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
      <h2 class="text-2xl font-bold text-gray-800">Performance Comparison</h2>
      <p class="text-gray-600 text-sm mt-1">Run 5 tests to compare slow vs fast endpoints</p>
    </div>
    <button on:click={runPerformanceTest} class="btn-primary">
      🚀 Run Performance Test
    </button>
  </div>

  {#if slowData.length > 0}
    <div class="grid grid-cols-2 gap-6 mt-6">
      <!-- Slow Endpoint Results -->
      <div class="bg-orange-50 p-4 rounded-lg border border-orange-200">
        <h3 class="font-semibold text-orange-700 mb-3">Slow Endpoint (Database)</h3>
        <div class="space-y-2">
          {#each slowData as result}
            <div class="flex justify-between text-sm">
              <span class="text-gray-700">{result.label}</span>
              <span class="font-bold text-orange-600">{result.time}ms</span>
            </div>
          {/each}
        </div>
        <div class="mt-4 pt-4 border-t border-orange-200">
          <p class="text-sm text-gray-600">Average Response Time</p>
          <p class="text-2xl font-bold text-orange-600">{avgSlow}ms</p>
        </div>
      </div>

      <!-- Fast Endpoint Results -->
      <div class="bg-green-50 p-4 rounded-lg border border-green-200">
        <h3 class="font-semibold text-green-700 mb-3">Fast Endpoint (Cached)</h3>
        <div class="space-y-2">
          {#each fastData as result}
            <div class="flex justify-between text-sm">
              <span class="text-gray-700">{result.label}</span>
              <span class="font-bold text-green-600">{result.time}ms</span>
            </div>
          {/each}
        </div>
        <div class="mt-4 pt-4 border-t border-green-200">
          <p class="text-sm text-gray-600">Average Response Time</p>
          <p class="text-2xl font-bold text-green-600">{avgFast}ms</p>
        </div>
      </div>
    </div>

    {#if avgSlow > 0 && avgFast > 0}
      <div class="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
        <p class="text-sm text-gray-600 mb-2">Performance Improvement</p>
        <div class="flex items-end gap-4">
          <div>
            <p class="text-3xl font-bold text-blue-600">
              {((1 - avgFast / avgSlow) * 100).toFixed(1)}%
            </p>
            <p class="text-sm text-gray-600">faster</p>
          </div>
          <div class="text-sm text-gray-600">
            <p>Slow: {avgSlow}ms → Fast: {avgFast}ms</p>
          </div>
        </div>
      </div>
    {/if}
  {/if}
</div>

<style></style>
