using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Messages;

namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Listeners;

public sealed class TradeListener : IListener<AlpacaTradeMessage>
{
    private readonly ILogger<TradeListener> _logger;
    private readonly IQueueTradeMessage _queueTradeMessage;

    public TradeListener(
        ILogger<TradeListener> logger,
        IQueueTradeMessage queueTradeMessage
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(queueTradeMessage);

        _logger = logger;
        _queueTradeMessage = queueTradeMessage;
    }

    public async Task HandleMessageAsync(
        AlpacaTradeMessage message,
        IWebSocketClient _,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle Alpaca trade message {@message}.", message);
        cancellationToken.ThrowIfCancellationRequested();

        var tradeMessage = new TradeMessage(
            Domain.Producer.Stocks,
            message.SymbolCode,
            null,
            message.SymbolCode,
            null,
            message.Price,
            message.Size,
            message.Timestamp,
            new Dictionary<string, object?>
            {
                { "exchange", message.ExchangeCode },
                { "tape", message.Tape },
                { "alpacaTradeId", message.TradeId }
            }
        );

        await _queueTradeMessage.QueueMessage(tradeMessage, cancellationToken);

        _logger.LogInformation("Successfully handled Alpaca trade message.");
    }
}