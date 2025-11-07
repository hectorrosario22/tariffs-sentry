<script>
  let { message = '', messageType = 'info' } = $props(); // 'success', 'danger', 'info'

  const bgColor = {
    success: 'bg-green-100 border-green-300 text-green-800',
    danger: 'bg-red-100 border-red-300 text-red-800',
    info: 'bg-blue-100 border-blue-300 text-blue-800'
  };

  const icon = {
    success: '✓',
    danger: '⚠️',
    info: 'ℹ️'
  };
</script>

{#if message}
  <div class={`fixed top-4 right-4 p-4 border rounded-lg ${bgColor[messageType]} shadow-lg animate-pulse`}>
    <div class="flex items-center gap-2">
      <span class="text-lg">{icon[messageType]}</span>
      <span>{message}</span>
    </div>
  </div>
{/if}

<style></style>
