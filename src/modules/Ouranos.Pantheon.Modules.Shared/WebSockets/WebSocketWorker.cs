using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets;

public sealed class WebSocketWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWebSocketClient _client;
    private readonly ILogger<WebSocketWorker> _logger;
    private readonly IOptions<WebSocketOptions> _options;

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

        _logger = logger;
        _client = client;
        _applicationLifetime = applicationLifetime;
        _options = options;
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

                await Task.Delay(
                    TimeSpan.FromSeconds(_options.Value.HealthCheckIntervalSeconds),
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cancellation requested, exiting.");
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception encountered, restarting.");

                await Task.Delay(
                    TimeSpan.FromSeconds(_options.Value.ErrorDelayIntervalSeconds),
                    cancellationToken
                );
            }
        }

        await _client.DisconnectAsync(cancellationToken);
        _applicationLifetime.StopApplication();
    }
}