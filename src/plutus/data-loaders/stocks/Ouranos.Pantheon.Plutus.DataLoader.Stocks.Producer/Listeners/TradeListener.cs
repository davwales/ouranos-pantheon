using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Plutus.DataLoader.Application.Interfaces.Trades;
using Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using TradeMessage = Ouranos.Pantheon.Plutus.DataLoader.Domain.Trades.TradeMessage;

namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Listeners;

public sealed class TradeListener : IListener<AlpacaTradeMessage>
{
    private readonly ILogger<TradeListener> _logger;
    private readonly IQueueTradeMessages _queueTradeMessages;

    public TradeListener(
        ILogger<TradeListener> logger,
        IQueueTradeMessages queueTradeMessages
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(queueTradeMessages);

        _logger = logger;
        _queueTradeMessages = queueTradeMessages;
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
            message.Price,
            message.Size,
            message.Timestamp,
            new AdditionalFields(
                Exchange: message.ExchangeCode,
                Tape: message.Tape,
                ExternalTradeId: message.TradeId.ToString()
            )
        );

        await _queueTradeMessages.QueueMessages([tradeMessage], cancellationToken);

        _logger.LogInformation("Successfully handled Alpaca trade message.");
    }
}