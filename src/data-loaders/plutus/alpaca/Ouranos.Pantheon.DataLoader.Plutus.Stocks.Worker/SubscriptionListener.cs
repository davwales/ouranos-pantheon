using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Messages;

namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker;

public sealed class SubscriptionListener : IListener<SubscriptionAckMessage>
{
    private readonly ILogger<SubscriptionListener> _logger;

    public SubscriptionListener(ILogger<SubscriptionListener> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task HandleMessageAsync(
        SubscriptionAckMessage message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle subscription acknowledgement message '{@message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Received subscription acknowledgement for trades '{trades}'.",
            string.Join(", ", message.Trades)
        );

        await Task.CompletedTask;
    }
}