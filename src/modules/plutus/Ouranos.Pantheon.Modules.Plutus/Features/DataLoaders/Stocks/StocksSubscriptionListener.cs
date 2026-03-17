using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

public sealed class StocksSubscriptionListener : IListener<SubscriptionAckMessage>
{
    private readonly ILogger<StocksSubscriptionListener> _logger;

    public StocksSubscriptionListener(ILogger<StocksSubscriptionListener> logger)
    {
        Guard.Against.Null(logger);
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

        await Task.CompletedTask;

        _logger.LogInformation(
            "Received subscription acknowledgement for trades '{trades}'.",
            string.Join(", ", message.Trades)
        );
    }
}
