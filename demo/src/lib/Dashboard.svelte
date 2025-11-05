<script>
  import Header from './Header.svelte';
  import MetricsSection from './MetricsSection.svelte';
  import PerformanceChart from './PerformanceChart.svelte';
  import ArchitectureInfo from './ArchitectureInfo.svelte';
  import NotificationArea from './NotificationArea.svelte';

  let message = '';
  let messageType = 'info';

  function showNotification(text, type = 'info') {
    message = text;
    messageType = type;
    setTimeout(() => {
      message = '';
    }, 5000);
  }
</script>

<div class="min-h-screen" style="background: linear-gradient(to bottom, #FFF5EB, #FFFBF5);">
  <Header />

  {#if message}
    <NotificationArea {message} {messageType} />
  {/if}

  <div class="container mx-auto px-4 py-8">
    <!-- Metrics Section -->
    <MetricsSection on:notification={(e) => showNotification(e.detail.message, e.detail.type)} />

    <!-- Performance Comparison Chart -->
    <div class="mt-8">
      <PerformanceChart on:notification={(e) => showNotification(e.detail.message, e.detail.type)} />
    </div>

    <!-- Architecture Information -->
    <div class="mt-8">
      <ArchitectureInfo />
    </div>
  </div>
</div>

<style></style>
