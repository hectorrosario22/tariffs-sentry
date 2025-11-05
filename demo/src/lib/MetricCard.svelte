<script>
  export let title = '';
  export let value = '-';
  export let label = '';
  export let loading = false;
  export let onTest = null;
  export let color = 'from-blue-400 to-blue-600';
</script>

<div class="card bg-gradient-to-br {color} text-white shadow-lg hover:shadow-xl transition-shadow">
  <div class="flex justify-between items-start mb-4">
    <div>
      <p class="text-sm font-semibold opacity-90">{title}</p>
      <p class="text-4xl font-bold mt-2">{value}</p>
    </div>
  </div>

  <p class="text-sm opacity-80 mb-4">{label}</p>

  {#if onTest}
    <button
      on:click={onTest}
      disabled={loading}
      class="btn-secondary w-full text-sm"
    >
      {loading ? '⏳ Testing...' : '🧪 Test Endpoint'}
    </button>
  {/if}
</div>

<style></style>
