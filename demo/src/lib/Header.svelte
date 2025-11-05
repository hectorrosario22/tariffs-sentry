<script>
  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';

  function openScalarDocs() {
    window.open(`${apiUrl}`, '_blank');
  }
</script>

<header style="background: linear-gradient(135deg, #F7931E 0%, #3B82F6 100%); box-shadow: 0 4px 12px rgba(247, 147, 30, 0.2);">
  <div class="container mx-auto px-4 py-6 flex justify-between items-center">
    <div>
      <h1 class="text-3xl font-bold text-white drop-shadow-md">⚡ High Performance Tariffs API</h1>
      <p class="text-white/90 text-sm mt-1 drop-shadow">Demonstrating Clean Architecture & Distributed Caching</p>
    </div>
    <button
      on:click={openScalarDocs}
      class="px-6 py-3 bg-white text-orange-600 font-semibold rounded-lg hover:bg-orange-50 transition-all shadow-md hover:shadow-lg"
    >
      📚 API Documentation
    </button>
  </div>
</header>

<style></style>
