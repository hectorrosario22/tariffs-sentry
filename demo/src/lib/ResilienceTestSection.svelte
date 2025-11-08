<script lang="ts">
	import RequestLog from './RequestLog.svelte';
	import RequestDetailModal from './RequestDetailModal.svelte';

	interface RequestLogEntry {
		id: string;
		status: number;
		timestamp: string;
		retryAfter?: number;
		limit?: number;
		remaining?: number;
		url?: string;
		requestHeaders?: Record<string, string>;
		responseHeaders?: Record<string, string>;
		responseBody?: any;
		latency?: number;
	}

	interface TestState {
		logs: RequestLogEntry[];
		isAutoTesting: boolean;
		testCount: number;
	}

	const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';

	let strictState: TestState = $state({
		logs: [],
		isAutoTesting: false,
		testCount: 0
	});

	let permissiveState: TestState = $state({
		logs: [],
		isAutoTesting: false,
		testCount: 0
	});

	let isModalOpen = $state(false);
	let selectedRequest: any = $state(null);

	function openDetailsModal(logEntry: RequestLogEntry) {
		selectedRequest = {
			url: logEntry.url || '',
			method: 'GET',
			requestHeaders: logEntry.requestHeaders || {},
			responseHeaders: logEntry.responseHeaders || {},
			responseBody: logEntry.responseBody || {},
			status: logEntry.status,
			latency: logEntry.latency
		};
		isModalOpen = true;
	}

	function closeModal() {
		isModalOpen = false;
		selectedRequest = null;
	}

	async function sendRequest(endpoint: 'slow' | 'fast', state: TestState) {
		try {
			const url = `${apiUrl}/api/v1/tariffs/${endpoint}`;
			const start = performance.now();
			const response = await fetch(url);
			const latency = performance.now() - start;

			const timestamp = new Date().toISOString();
			const status = response.status;

			const limit = response.headers.get('ratelimit-limit');
			const remaining = response.headers.get('ratelimit-remaining');
			const retryAfter = response.headers.get('retry-after');

			// Capturar todos los headers de respuesta
			const responseHeaders: Record<string, string> = {};
			response.headers.forEach((value, key) => {
				responseHeaders[key] = value;
			});

			// Capturar body de respuesta
			let responseBody: any = {};
			try {
				responseBody = await response.json();
			} catch {
				responseBody = { error: 'Could not parse response body' };
			}

			const entry: RequestLogEntry = {
				id: `${Date.now()}-${Math.random()}`,
				status,
				timestamp,
				retryAfter: retryAfter ? parseInt(retryAfter) : undefined,
				limit: limit ? parseInt(limit) : undefined,
				remaining: remaining ? parseInt(remaining) : undefined,
				url,
				requestHeaders: {},
				responseHeaders,
				responseBody,
				latency: Math.round(latency)
			};

			state.logs = [entry, ...state.logs];
		} catch (error) {
			const entry: RequestLogEntry = {
				id: `${Date.now()}-${Math.random()}`,
				status: 0,
				timestamp: new Date().toISOString()
			};
			state.logs = [entry, ...state.logs];
		}
	}

	async function singleRequest(endpoint: 'slow' | 'fast', state: TestState) {
		state.testCount += 1;
		await sendRequest(endpoint, state);
	}

	async function autoTest(endpoint: 'slow' | 'fast', state: TestState) {
		if (state.isAutoTesting) {
			state.isAutoTesting = false;
			return;
		}

		state.isAutoTesting = true;
		state.testCount = 0;
		state.logs = [];

		for (let i = 0; i < 10 && state.isAutoTesting; i++) {
			state.testCount += 1;
			await sendRequest(endpoint, state);
			await new Promise(resolve => setTimeout(resolve, 300));
		}

		state.isAutoTesting = false;
	}

	function clearLogs(state: TestState) {
		state.logs = [];
		state.testCount = 0;
	}
</script>

<section class="resilience-section">
	<div class="section-header">
		<h2>Resilience Testing</h2>
		<p class="subtitle">Test endpoint rate limits with different policies</p>
	</div>

	<div class="test-grid">
		<!-- Strict Policy Column -->
		<div class="test-column">
			<div class="column-header">
				<h3>Strict Policy</h3>
				<span class="policy-badge strict">2 req/min</span>
			</div>

			<div class="endpoint-info">
				<p class="endpoint-path">/api/v1/tariffs/slow</p>
				<p class="description">Limit: 2 requests per minute per IP</p>
			</div>

			<div class="button-group">
				<button
					class="btn btn-primary"
					onclick={() => singleRequest('slow', strictState)}
					disabled={strictState.isAutoTesting}
				>
					Send Request
				</button>

				<button
					class="btn btn-secondary"
					class:active={strictState.isAutoTesting}
					onclick={() => autoTest('slow', strictState)}
				>
					{strictState.isAutoTesting ? 'Stopping...' : 'Auto-Test (10x)'}
				</button>

				<button
					class="btn btn-outline"
					onclick={() => clearLogs(strictState)}
					disabled={strictState.logs.length === 0}
				>
					Clear
				</button>
			</div>

			<div class="test-counter">
				<span class="label">Requests sent:</span>
				<span class="counter">{strictState.testCount}</span>
			</div>

			<div class="logs-container">
				{#if strictState.logs.length === 0}
					<div class="empty-state">
						<p>No requests sent yet</p>
					</div>
				{:else}
					<div class="logs-list">
						{#each strictState.logs as log (log.id)}
							<RequestLog
								status={log.status}
								timestamp={log.timestamp}
								retryAfter={log.retryAfter}
								limit={log.limit}
								remaining={log.remaining}
								onShowDetails={() => openDetailsModal(log)}
							/>
						{/each}
					</div>
				{/if}
			</div>
		</div>

		<!-- Permissive Policy Column -->
		<div class="test-column">
			<div class="column-header">
				<h3>Permissive Policy</h3>
				<span class="policy-badge permissive">20 req/min</span>
			</div>

			<div class="endpoint-info">
				<p class="endpoint-path">/api/v1/tariffs/fast</p>
				<p class="description">Limit: 20 requests per minute per IP</p>
			</div>

			<div class="button-group">
				<button
					class="btn btn-primary"
					onclick={() => singleRequest('fast', permissiveState)}
					disabled={permissiveState.isAutoTesting}
				>
					Send Request
				</button>

				<button
					class="btn btn-secondary"
					class:active={permissiveState.isAutoTesting}
					onclick={() => autoTest('fast', permissiveState)}
				>
					{permissiveState.isAutoTesting ? 'Stopping...' : 'Auto-Test (10x)'}
				</button>

				<button
					class="btn btn-outline"
					onclick={() => clearLogs(permissiveState)}
					disabled={permissiveState.logs.length === 0}
				>
					Clear
				</button>
			</div>

			<div class="test-counter">
				<span class="label">Requests sent:</span>
				<span class="counter">{permissiveState.testCount}</span>
			</div>

			<div class="logs-container">
				{#if permissiveState.logs.length === 0}
					<div class="empty-state">
						<p>No requests sent yet</p>
					</div>
				{:else}
					<div class="logs-list">
						{#each permissiveState.logs as log (log.id)}
							<RequestLog
								status={log.status}
								timestamp={log.timestamp}
								retryAfter={log.retryAfter}
								limit={log.limit}
								remaining={log.remaining}
								onShowDetails={() => openDetailsModal(log)}
							/>
						{/each}
					</div>
				{/if}
			</div>
		</div>
	</div>
</section>

<!-- Modal de detalles -->
<RequestDetailModal
	isOpen={isModalOpen}
	onClose={closeModal}
	requestDetails={selectedRequest}
/>

<style>
	.resilience-section {
		padding: 2rem;
		border-radius: 1rem;
		background: linear-gradient(135deg, rgba(15, 23, 42, 0.8), rgba(30, 41, 59, 0.8));
		border: 1px solid rgba(255, 255, 255, 0.1);
	}

	.section-header {
		margin-bottom: 2rem;
		text-align: center;
	}

	.section-header h2 {
		margin: 0 0 0.5rem 0;
		font-size: 2rem;
		font-weight: 700;
		background: linear-gradient(135deg, #f7931e, #3b82f6);
		-webkit-background-clip: text;
		-webkit-text-fill-color: transparent;
		background-clip: text;
	}

	.subtitle {
		margin: 0;
		color: rgba(255, 255, 255, 0.6);
		font-size: 1rem;
	}

	.test-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
		gap: 2rem;
	}

	.test-column {
		padding: 1.5rem;
		border-radius: 0.75rem;
		background: rgba(15, 23, 42, 0.5);
		border: 1px solid rgba(255, 255, 255, 0.1);
		transition: all 0.3s ease;
	}

	.test-column:hover {
		border-color: rgba(255, 255, 255, 0.2);
		background: rgba(15, 23, 42, 0.7);
	}

	.column-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 1rem;
		gap: 1rem;
	}

	.column-header h3 {
		margin: 0;
		font-size: 1.25rem;
		font-weight: 600;
		color: #fff;
	}

	.policy-badge {
		padding: 0.375rem 0.75rem;
		border-radius: 0.375rem;
		font-size: 0.75rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.05em;
		white-space: nowrap;
	}

	.policy-badge.strict {
		background: rgba(239, 68, 68, 0.15);
		color: #fca5a5;
		border: 1px solid rgba(239, 68, 68, 0.4);
	}

	.policy-badge.permissive {
		background: rgba(34, 197, 94, 0.15);
		color: #86efac;
		border: 1px solid rgba(34, 197, 94, 0.4);
	}

	.endpoint-info {
		margin-bottom: 1.5rem;
		padding: 1rem;
		border-radius: 0.5rem;
		background: rgba(255, 255, 255, 0.05);
		border: 1px solid rgba(255, 255, 255, 0.1);
	}

	.endpoint-path {
		margin: 0 0 0.5rem 0;
		font-family: 'Courier New', monospace;
		font-size: 0.875rem;
		font-weight: 600;
		color: #60a5fa;
	}

	.description {
		margin: 0;
		font-size: 0.875rem;
		color: rgba(255, 255, 255, 0.6);
		line-height: 1.5;
	}

	.button-group {
		display: flex;
		gap: 0.75rem;
		margin-bottom: 1.5rem;
		flex-wrap: wrap;
	}

	.btn {
		flex: 1;
		min-width: 120px;
		padding: 0.625rem 1rem;
		border: none;
		border-radius: 0.5rem;
		font-size: 0.875rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.2s ease;
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.btn-primary {
		background: linear-gradient(135deg, #f7931e, #3b82f6);
		color: #fff;
		box-shadow: 0 4px 12px rgba(247, 147, 30, 0.3);
	}

	.btn-primary:hover:not(:disabled) {
		transform: translateY(-2px);
		box-shadow: 0 6px 16px rgba(247, 147, 30, 0.4);
	}

	.btn-primary:active:not(:disabled) {
		transform: translateY(0);
	}

	.btn-secondary {
		background: rgba(59, 130, 246, 0.2);
		color: #60a5fa;
		border: 1px solid rgba(59, 130, 246, 0.4);
	}

	.btn-secondary:hover:not(:disabled) {
		background: rgba(59, 130, 246, 0.3);
		border-color: rgba(59, 130, 246, 0.6);
	}

	.btn-secondary.active {
		background: rgba(59, 130, 246, 0.4);
		border-color: rgba(59, 130, 246, 0.8);
	}

	.btn-outline {
		background: transparent;
		color: rgba(255, 255, 255, 0.6);
		border: 1px solid rgba(255, 255, 255, 0.2);
	}

	.btn-outline:hover:not(:disabled) {
		background: rgba(255, 255, 255, 0.05);
		border-color: rgba(255, 255, 255, 0.4);
		color: #fff;
	}

	.test-counter {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		margin-bottom: 1.5rem;
		padding: 0.75rem;
		border-radius: 0.5rem;
		background: rgba(255, 255, 255, 0.05);
	}

	.test-counter .label {
		color: rgba(255, 255, 255, 0.6);
		font-size: 0.875rem;
		font-weight: 500;
	}

	.test-counter .counter {
		background: linear-gradient(135deg, #f7931e, #3b82f6);
		-webkit-background-clip: text;
		-webkit-text-fill-color: transparent;
		background-clip: text;
		font-weight: 700;
		font-size: 1.25rem;
		font-family: 'Courier New', monospace;
	}

	.logs-container {
		height: 400px;
		border-radius: 0.5rem;
		background: rgba(0, 0, 0, 0.3);
		border: 1px solid rgba(255, 255, 255, 0.1);
		overflow: hidden;
		display: flex;
		flex-direction: column;
	}

	.empty-state {
		display: flex;
		align-items: center;
		justify-content: center;
		height: 100%;
		color: rgba(255, 255, 255, 0.4);
		text-align: center;
		padding: 2rem;
	}

	.empty-state p {
		margin: 0;
		font-size: 0.875rem;
	}

	.logs-list {
		flex: 1;
		overflow-y: auto;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
		padding: 0.75rem;
	}

	.logs-list :global(> *) {
		flex-shrink: 0;
	}

	/* Custom scrollbar */
	.logs-list::-webkit-scrollbar {
		width: 8px;
	}

	.logs-list::-webkit-scrollbar-track {
		background: rgba(255, 255, 255, 0.05);
	}

	.logs-list::-webkit-scrollbar-thumb {
		background: rgba(255, 255, 255, 0.2);
		border-radius: 4px;
	}

	.logs-list::-webkit-scrollbar-thumb:hover {
		background: rgba(255, 255, 255, 0.3);
	}

	@media (max-width: 900px) {
		.test-grid {
			grid-template-columns: 1fr;
		}
	}

	@media (max-width: 640px) {
		.resilience-section {
			padding: 1rem;
		}

		.section-header h2 {
			font-size: 1.5rem;
		}

		.button-group {
			flex-direction: column;
		}

		.btn {
			min-width: 100%;
		}
	}
</style>
