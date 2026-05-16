using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets;

public sealed class WebSocketWorker(
    ILogger<WebSocketWorker> logger,
    IWebSocketClient client,
    IOptions<WebSocketOptions> options,
    string workerName = "unknown",
    WebSocketHealthState? health = null
) : BackgroundService
{
    private readonly IWebSocketClient _client = Guard.Against.Null(client);
    private readonly ILogger<WebSocketWorker> _logger = Guard.Against.Null(logger);
    private readonly IOptions<WebSocketOptions> _options = options;
    private readonly WebSocketHealthState? _health = health;
    private readonly string _workerName = workerName;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var reconnectDelay = TimeSpan.FromSeconds(_options.Value.ReconnectBaseDelaySeconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            var connected = await TryConnectAndMonitorAsync(reconnectDelay, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await DisconnectAsync();

            if (!await TryWaitForReconnectAsync(reconnectDelay, cancellationToken))
            {
                break;
            }

            reconnectDelay = connected
                ? TimeSpan.FromSeconds(_options.Value.ReconnectBaseDelaySeconds)
                : AdvanceBackoff(reconnectDelay);
        }

        await DisconnectAsync();
        _logger.LogInformation("WebSocketWorker stopped for host '{host}'.", _options.Value.Host);
    }

    private async Task<bool> TryConnectAndMonitorAsync(
        TimeSpan reconnectDelay,
        CancellationToken cancellationToken
    )
    {
        try
        {
            _logger.LogInformation(
                "WebSocketWorker connecting to '{host}' (next backoff: {delay}s).",
                _options.Value.Host,
                reconnectDelay.TotalSeconds
            );

            await _client.ConnectAsync(cancellationToken);
            _logger.LogInformation("WebSocketWorker connected to '{host}'.", _options.Value.Host);
            _health?.Report(_workerName, true);

            await MonitorConnectionAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _health?.Report(_workerName, false);
            return false;
        }
        catch (Exception ex)
        {
            _health?.Report(_workerName, false);
            _logger.LogError(
                ex,
                "WebSocketWorker error for '{host}', reconnecting in {delay}s.",
                _options.Value.Host,
                reconnectDelay.TotalSeconds
            );
            return false;
        }
    }

    private async Task MonitorConnectionAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _client.IsListening)
        {
            await Task.Delay(
                TimeSpan.FromSeconds(_options.Value.HealthCheckIntervalSeconds),
                cancellationToken
            );
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            _health?.Report(_workerName, false);
            _logger.LogWarning(
                "WebSocketWorker lost connection to '{host}', reconnecting.",
                _options.Value.Host
            );
        }
    }

    private async Task<bool> TryWaitForReconnectAsync(
        TimeSpan delay,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private async Task DisconnectAsync()
    {
        try
        {
            await _client.DisconnectAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error disconnecting WebSocket.");
        }
    }

    private TimeSpan AdvanceBackoff(TimeSpan current)
    {
        var maxDelay = TimeSpan.FromSeconds(_options.Value.ReconnectMaxDelaySeconds);
        return TimeSpan.FromSeconds(Math.Min(current.TotalSeconds * 2, maxDelay.TotalSeconds));
    }
}
