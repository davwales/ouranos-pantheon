using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Messages;

namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker;

public sealed class ErrorListener : IListener<ErrorMessage>
{
    private readonly ILogger<ErrorListener> _logger;

    public ErrorListener(ILogger<ErrorListener> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task HandleMessageAsync(
        ErrorMessage message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle error message '{@message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        const int notAuthorizedCode = 401;
        if (message.Code is notAuthorizedCode)
        {
            _logger.LogWarning("Failed to authorized with Alpaca.");
            await client.DisconnectAsync(cancellationToken);
        }

        const int connectionLimitExceededCode = 406;
        if (message.Code is connectionLimitExceededCode)
        {
            _logger.LogWarning("Connection limit exceeded with Alpaca.");
            await client.DisconnectAsync(cancellationToken);
        }

        _logger.LogDebug("Successfully handled error message.");
    }
}