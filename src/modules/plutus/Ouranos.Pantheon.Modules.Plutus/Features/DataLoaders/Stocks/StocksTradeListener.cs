using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

public sealed class StocksTradeListener : IListener<AlpacaTradeMessage>
{
    private readonly ILogger<StocksTradeListener> _logger;
    private readonly IQueueTradeMessages _queueTradeMessages;

    public StocksTradeListener(
        ILogger<StocksTradeListener> logger,
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
            Producer.Stocks,
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
