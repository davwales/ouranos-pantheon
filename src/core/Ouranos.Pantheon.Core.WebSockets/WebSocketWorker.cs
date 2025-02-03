using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets;

public sealed class WebSocketWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWebSocketClient _client;
    private readonly TimeSpan _healthCheckInterval = TimeSpan.FromSeconds(5);
    private readonly ILogger<WebSocketWorker> _logger;

    public WebSocketWorker(
        ILogger<WebSocketWorker> logger,
        IWebSocketClient client,
        IHostApplicationLifetime applicationLifetime
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(client);
        Guard.Against.Null(applicationLifetime);

        _logger = logger;
        _client = client;
        _applicationLifetime = applicationLifetime;
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
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        await _client.DisconnectAsync(cancellationToken);
        _applicationLifetime.StopApplication();
    }
}