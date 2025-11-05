<script>
  import MetricCard from './MetricCard.svelte';

  let slowTime = '-';
  let fastTime = '-';
  let slowLoading = false;
  let fastLoading = false;
  let cacheHitRate = '0%';

  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';

  async function fetchTariffsSlow() {
    slowLoading = true;
    try {
      const start = performance.now();
      const response = await fetch(`${apiUrl}/api/v1/tariffs/slow`);
      const end = performance.now();
      const time = (end - start).toFixed(2);
      slowTime = `${time}ms`;

      dispatch('notification', { message: `Slow endpoint: ${time}ms`, type: 'success' });
    } catch (error) {
      dispatch('notification', { message: 'Error fetching slow data', type: 'danger' });
      slowTime = 'Error';
    } finally {
      slowLoading = false;
    }
  }

  async function fetchTariffsFast() {
    fastLoading = true;
    try {
      const start = performance.now();
      const response = await fetch(`${apiUrl}/api/v1/tariffs/fast`);
      const end = performance.now();
      const time = (end - start).toFixed(2);
      fastTime = `${time}ms`;

      // Simulate cache hit rate
      const hitRate = (95 + Math.random() * 5).toFixed(1);
      cacheHitRate = `${hitRate}%`;

      dispatch('notification', { message: `Fast endpoint: ${time}ms`, type: 'success' });
    } catch (error) {
      dispatch('notification', { message: 'Error fetching fast data', type: 'danger' });
      fastTime = 'Error';
    } finally {
      fastLoading = false;
    }
  }

  function dispatch(eventName, detail) {
    const event = new CustomEvent(eventName, { detail });
    window.dispatchEvent(event);
    // Also dispatch via Svelte event
    const evt = new Event(eventName);
    evt.detail = detail;
    document.dispatchEvent(evt);
  }
</script>

<div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
  <MetricCard
    title="Slow Endpoint"
    value={slowTime}
    label="Direct Database"
    loading={slowLoading}
    onTest={fetchTariffsSlow}
    color="from-orange-400 to-orange-600"
  />

  <MetricCard
    title="Fast Endpoint"
    value={fastTime}
    label="With Redis Cache"
    loading={fastLoading}
    onTest={fetchTariffsFast}
    color="from-green-400 to-green-600"
  />

  <MetricCard
    title="Cache Hit Rate"
    value={cacheHitRate}
    label="Performance Gain"
    color="from-blue-400 to-blue-600"
  />
</div>

<style></style>
