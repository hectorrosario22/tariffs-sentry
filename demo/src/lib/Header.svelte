<script>
  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';

  function openScalarDocs() {
    window.open(`${apiUrl}`, '_blank');
  }
</script>

<header class="bg-white shadow">
  <div class="container mx-auto px-4 py-6 flex justify-between items-center">
    <div>
      <h1 class="text-3xl font-bold text-blue-600">High Performance Tariffs API</h1>
      <p class="text-gray-600 text-sm mt-1">Demonstrating Clean Architecture & Distributed Caching</p>
    </div>
    <button
      on:click={openScalarDocs}
      class="btn-primary"
    >
      📚 API Documentation
    </button>
  </div>
</header>

<style></style>
