<script lang="ts">
	interface Props {
		isOpen: boolean;
		onClose: () => void;
		requestDetails: {
			url: string;
			method: string;
			requestHeaders: Record<string, string>;
			responseHeaders: Record<string, string>;
			responseBody: any;
			status: number;
			latency?: number;
		} | null;
	}

	let { isOpen = false, onClose, requestDetails }: Props = $props();

	function handleOutsideClick(event: MouseEvent) {
		if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
			onClose();
		}
	}

	function formatHeaders(headers: Record<string, string>): string {
		return Object.entries(headers)
			.map(([key, value]) => `${key}: ${value}`)
			.join('\n');
	}

	function formatBody(body: any): string {
		if (typeof body === 'object') {
			return JSON.stringify(body, null, 2);
		}
		return String(body);
	}

	// Destacar headers importantes
	function isImportantHeader(key: string): boolean {
		const important = [
			'ratelimit-limit',
			'ratelimit-remaining',
			'ratelimit-reset',
			'retry-after',
			'cache-control',
			'x-bypass-ratelimit',
			'x-bypass-test'
		];
		return important.includes(key.toLowerCase());
	}
</script>

{#if isOpen && requestDetails}
	<div class="modal-backdrop" onclick={handleOutsideClick}>
		<div class="modal-content" onclick={(e) => e.stopPropagation()}>
			<div class="modal-header">
				<div>
					<h2 class="modal-title">Request Details</h2>
					<p class="modal-subtitle">
						<span class="status-badge" class:success={requestDetails.status === 200} class:error={requestDetails.status >= 400}>
							{requestDetails.status}
						</span>
						{#if requestDetails.latency}
							<span class="latency-badge">{requestDetails.latency}ms</span>
						{/if}
					</p>
				</div>
				<button class="close-button" onclick={onClose}>
					<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
						<line x1="18" y1="6" x2="6" y2="18"></line>
						<line x1="6" y1="6" x2="18" y2="18"></line>
					</svg>
				</button>
			</div>

			<div class="modal-body">
				<!-- URL Section -->
				<div class="section">
					<h3 class="section-title">Requested URL</h3>
					<div class="code-block">
						<code>{requestDetails.method} {requestDetails.url}</code>
					</div>
				</div>

				<!-- Request Headers Section -->
				<div class="section">
					<h3 class="section-title">Request Headers</h3>
					<div class="headers-grid">
						{#each Object.entries(requestDetails.requestHeaders) as [key, value]}
							<div class="header-row" class:important={isImportantHeader(key)}>
								<span class="header-key">{key}</span>
								<span class="header-value">{value}</span>
							</div>
						{:else}
							<p class="empty-message">No additional headers sent</p>
						{/each}
					</div>
				</div>

				<!-- Response Headers Section -->
				<div class="section">
					<h3 class="section-title">Response Headers</h3>
					<div class="headers-grid">
						{#each Object.entries(requestDetails.responseHeaders) as [key, value]}
							<div class="header-row" class:important={isImportantHeader(key)}>
								<span class="header-key">{key}</span>
								<span class="header-value">{value}</span>
							</div>
						{:else}
							<p class="empty-message">No response headers received</p>
						{/each}
					</div>
				</div>

				<!-- Response Body Section -->
				<div class="section">
					<h3 class="section-title">Response Body</h3>
					<div class="code-block body">
						<pre><code>{formatBody(requestDetails.responseBody)}</code></pre>
					</div>
				</div>
			</div>
		</div>
	</div>
{/if}

<style>
	.modal-backdrop {
		position: fixed;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background: rgba(0, 0, 0, 0.75);
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 1000;
		padding: 2rem;
		overflow-y: auto;
	}

	.modal-content {
		background: linear-gradient(135deg, rgba(15, 23, 42, 0.98), rgba(30, 41, 59, 0.98));
		border: 1px solid rgba(255, 255, 255, 0.2);
		border-radius: 1rem;
		max-width: 900px;
		width: 100%;
		max-height: 90vh;
		overflow: hidden;
		display: flex;
		flex-direction: column;
		box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
	}

	.modal-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		padding: 1.5rem;
		border-bottom: 1px solid rgba(255, 255, 255, 0.1);
	}

	.modal-title {
		margin: 0 0 0.5rem 0;
		font-size: 1.5rem;
		font-weight: 700;
		background: linear-gradient(135deg, #f7931e, #3b82f6);
		-webkit-background-clip: text;
		-webkit-text-fill-color: transparent;
		background-clip: text;
	}

	.modal-subtitle {
		margin: 0;
		display: flex;
		gap: 0.75rem;
		align-items: center;
	}

	.status-badge {
		display: inline-block;
		padding: 0.25rem 0.75rem;
		border-radius: 0.375rem;
		font-size: 0.875rem;
		font-weight: 600;
		font-family: 'Courier New', monospace;
	}

	.status-badge.success {
		background: rgba(34, 197, 94, 0.2);
		color: #22c55e;
		border: 1px solid rgba(34, 197, 94, 0.5);
	}

	.status-badge.error {
		background: rgba(239, 68, 68, 0.2);
		color: #ef4444;
		border: 1px solid rgba(239, 68, 68, 0.5);
	}

	.latency-badge {
		padding: 0.25rem 0.75rem;
		border-radius: 0.375rem;
		font-size: 0.875rem;
		font-weight: 600;
		background: rgba(59, 130, 246, 0.2);
		color: #60a5fa;
		border: 1px solid rgba(59, 130, 246, 0.5);
		font-family: 'Courier New', monospace;
	}

	.close-button {
		background: transparent;
		border: none;
		color: rgba(255, 255, 255, 0.6);
		cursor: pointer;
		padding: 0.5rem;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: 0.375rem;
		transition: all 0.2s ease;
	}

	.close-button:hover {
		background: rgba(255, 255, 255, 0.1);
		color: #fff;
	}

	.modal-body {
		flex: 1;
		overflow-y: auto;
		padding: 1.5rem;
	}

	.section {
		margin-bottom: 1.5rem;
	}

	.section:last-child {
		margin-bottom: 0;
	}

	.section-title {
		margin: 0 0 0.75rem 0;
		font-size: 1rem;
		font-weight: 600;
		color: #fff;
		text-transform: uppercase;
		letter-spacing: 0.05em;
		font-size: 0.875rem;
	}

	.code-block {
		background: rgba(0, 0, 0, 0.4);
		border: 1px solid rgba(255, 255, 255, 0.1);
		border-radius: 0.5rem;
		padding: 1rem;
		overflow-x: auto;
	}

	.code-block code {
		font-family: 'Courier New', monospace;
		font-size: 0.875rem;
		color: #60a5fa;
		white-space: pre-wrap;
		word-break: break-all;
	}

	.code-block.body {
		max-height: 300px;
		overflow-y: auto;
	}

	.code-block.body pre {
		margin: 0;
	}

	.code-block.body code {
		color: #86efac;
		white-space: pre;
		word-break: normal;
	}

	.headers-grid {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.header-row {
		display: grid;
		grid-template-columns: minmax(200px, auto) 1fr;
		gap: 1rem;
		padding: 0.75rem;
		background: rgba(0, 0, 0, 0.3);
		border: 1px solid rgba(255, 255, 255, 0.1);
		border-radius: 0.375rem;
		align-items: start;
	}

	.header-row.important {
		background: rgba(247, 147, 30, 0.1);
		border-color: rgba(247, 147, 30, 0.3);
	}

	.header-key {
		font-family: 'Courier New', monospace;
		font-size: 0.875rem;
		font-weight: 600;
		color: #f7931e;
		word-break: break-word;
	}

	.header-row.important .header-key {
		color: #fbbf24;
	}

	.header-value {
		font-family: 'Courier New', monospace;
		font-size: 0.875rem;
		color: rgba(255, 255, 255, 0.8);
		word-break: break-word;
	}

	.empty-message {
		margin: 0;
		padding: 1rem;
		text-align: center;
		color: rgba(255, 255, 255, 0.4);
		font-size: 0.875rem;
	}

	/* Custom scrollbar */
	.modal-body::-webkit-scrollbar,
	.code-block.body::-webkit-scrollbar {
		width: 8px;
		height: 8px;
	}

	.modal-body::-webkit-scrollbar-track,
	.code-block.body::-webkit-scrollbar-track {
		background: rgba(255, 255, 255, 0.05);
	}

	.modal-body::-webkit-scrollbar-thumb,
	.code-block.body::-webkit-scrollbar-thumb {
		background: rgba(255, 255, 255, 0.2);
		border-radius: 4px;
	}

	.modal-body::-webkit-scrollbar-thumb:hover,
	.code-block.body::-webkit-scrollbar-thumb:hover {
		background: rgba(255, 255, 255, 0.3);
	}

	@media (max-width: 640px) {
		.modal-backdrop {
			padding: 1rem;
		}

		.modal-content {
			max-height: 95vh;
		}

		.header-row {
			grid-template-columns: 1fr;
			gap: 0.5rem;
		}
	}
</style>
