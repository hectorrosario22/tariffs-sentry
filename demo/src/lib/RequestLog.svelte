<script lang="ts">
	interface Props {
		status: number;
		timestamp: string;
		retryAfter?: number;
		limit?: number;
		remaining?: number;
		onShowDetails?: () => void;
	}

	let { status, timestamp, retryAfter, limit, remaining, onShowDetails }: Props = $props();

	const isSuccess = status === 200;
	const isRateLimited = status === 429;
	const statusLabel = isSuccess ? '200 OK' : isRateLimited ? '429 Too Many Requests' : `${status}`;

	function formatTime(dateString: string): string {
		const date = new Date(dateString);
		return date.toLocaleTimeString('es-ES', {
			hour: '2-digit',
			minute: '2-digit',
			second: '2-digit'
		});
	}
</script>

<div class="request-log">
	<div class="log-status">
		<span
			class="status-badge"
			class:success={isSuccess}
			class:error={isRateLimited}
		>
			{statusLabel}
		</span>
	</div>

	<div class="log-details">
		<p class="timestamp">
			<span class="label">Time:</span>
			<span class="value">{formatTime(timestamp)}</span>
		</p>

		{#if isRateLimited && retryAfter}
			<p class="retry-after">
				<span class="label">Retry After:</span>
				<span class="value">{retryAfter}s</span>
			</p>
		{/if}

		{#if limit !== undefined && remaining !== undefined}
			<p class="rate-limit-info">
				<span class="label">Limit / Remaining:</span>
				<span class="value">{remaining}/{limit}</span>
			</p>
		{/if}
	</div>

	{#if onShowDetails}
		<div class="log-actions">
			<button
				class="details-button"
				onclick={onShowDetails}
				title="View full details"
			>
				🔍
			</button>
		</div>
	{/if}
</div>

<style>
	.request-log {
		display: flex;
		align-items: center;
		gap: 1rem;
		padding: 0.75rem;
		border-radius: 0.5rem;
		background: rgba(15, 23, 42, 0.5);
		border: 1px solid rgba(255, 255, 255, 0.1);
		transition: all 0.2s ease;
	}

	.request-log:hover {
		background: rgba(15, 23, 42, 0.7);
		border-color: rgba(255, 255, 255, 0.2);
	}

	.log-status {
		flex-shrink: 0;
	}

	.status-badge {
		display: inline-block;
		padding: 0.25rem 0.75rem;
		border-radius: 0.375rem;
		font-size: 0.875rem;
		font-weight: 600;
		text-transform: uppercase;
		letter-spacing: 0.05em;
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

	.log-details {
		flex: 1;
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
		gap: 1rem;
		font-size: 0.875rem;
	}

	.log-details p {
		margin: 0;
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}

	.label {
		color: rgba(255, 255, 255, 0.6);
		font-weight: 500;
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.value {
		color: #fff;
		font-weight: 600;
		font-family: 'Courier New', monospace;
	}

	.timestamp {
	}

	.retry-after {
	}

	.rate-limit-info {
	}

	.log-actions {
		flex-shrink: 0;
		display: flex;
		align-items: center;
	}

	.details-button {
		background: rgba(59, 130, 246, 0.2);
		border: 1px solid rgba(59, 130, 246, 0.4);
		color: #60a5fa;
		padding: 0.5rem;
		border-radius: 0.375rem;
		cursor: pointer;
		font-size: 1rem;
		transition: all 0.2s ease;
		display: flex;
		align-items: center;
		justify-content: center;
		min-width: 36px;
		height: 36px;
	}

	.details-button:hover {
		background: rgba(59, 130, 246, 0.3);
		border-color: rgba(59, 130, 246, 0.6);
		transform: scale(1.1);
	}

	.details-button:active {
		transform: scale(0.95);
	}

	@media (max-width: 640px) {
		.request-log {
			flex-direction: column;
			align-items: flex-start;
		}

		.log-details {
			grid-template-columns: 1fr;
			width: 100%;
		}

		.log-actions {
			width: 100%;
			justify-content: center;
			margin-top: 0.5rem;
		}
	}
</style>
