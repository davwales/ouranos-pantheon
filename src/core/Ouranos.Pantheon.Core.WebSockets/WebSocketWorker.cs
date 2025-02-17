using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets;

public sealed class WebSocketWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWebSocketClient _client;
    private readonly TimeSpan _errorDelayInterval;
    private readonly TimeSpan _healthCheckInterval;
    private readonly ILogger<WebSocketWorker> _logger;

    public WebSocketWorker(
        ILogger<WebSocketWorker> logger,
        IWebSocketClient client,
        IHostApplicationLifetime applicationLifetime,
        IOptions<WebSocketOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(client);
        Guard.Against.Null(applicationLifetime);
        Guard.Against.Null(options?.Value);

        _logger = logger;
        _client = client;
        _applicationLifetime = applicationLifetime;
        _healthCheckInterval = TimeSpan.FromSeconds(options.Value.HealthCheckIntervalSeconds);
        _errorDelayInterval = TimeSpan.FromSeconds(options.Value.ErrorDelayIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _client.ConnectAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsListening)
                {
                    _logger.LogError("Web socket client is not listening, exiting.");
                    break;
                }

                await Task.Delay(_healthCheckInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cancellation requested, exiting.");
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception encountered, restarting.");
                await Task.Delay(_errorDelayInterval, cancellationToken);
            }
        }

        await _client.DisconnectAsync(cancellationToken);
        _applicationLifetime.StopApplication();
    }
}