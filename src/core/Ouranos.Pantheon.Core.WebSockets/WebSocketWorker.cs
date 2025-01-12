using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets;

public sealed class WebSocketWorker : BackgroundService
{
    private readonly IWebSocketClient _client;
    private readonly ILogger<WebSocketWorker> _logger;

    public WebSocketWorker(
        ILogger<WebSocketWorker> logger,
        IWebSocketClient client
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(client);

        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_client.State != WebSocketState.Open)
                {
                    _logger.LogDebug("Web socket client was not open, attempting to reconnect.");
                    await _client.ConnectAsync(cancellationToken);
                }

                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Web socket connection error.");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        await _client.DisconnectAsync(cancellationToken);
    }
}