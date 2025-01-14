using MassTransit;
using MediatR;
using Ouranos.Pantheon.Core.Common.AsyncLocks;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Commands.Symbols.UpsertSymbol;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Commands.Trades.InsertTrade;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer;

public sealed class TradeConsumer : IConsumer<TradeMessage>
{
    private readonly IKeyedAsyncLock<string> _itemLock;
    private readonly ILogger<TradeConsumer> _logger;
    private readonly Dictionary<Producer, Id<Market>> _marketMap;
    private readonly IMediator _mediator;

    public TradeConsumer(
        ILogger<TradeConsumer> logger,
        IMediator mediator,
        IConfiguration configuration,
        IKeyedAsyncLock<string> itemLock
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(itemLock);

        _logger = logger;
        _mediator = mediator;
        _itemLock = itemLock;
        _marketMap = configuration.GetSection("Ouranos:Markets").Get<Dictionary<Producer, string>>()
                         ?.ToDictionary(x => x.Key, x => new Id<Market>(x.Value))
                     ?? throw new InvalidOperationException("Cannot find market map in configuration.");
    }

    public async Task Consume(ConsumeContext<TradeMessage> context)
    {
        _logger.LogTrace("Attempting to consume trade message '{@message}'.", context.Message);

        if (!_marketMap.TryGetValue(context.Message.Producer, out var marketId))
        {
            throw new InvalidOperationException("Cannot find market for this message.");
        }

        // If we allow multiple messages for the same symbol to be processed concurrently, we run the risk of 
        // multiple symbols being inserted. Thus, we lock the upsert for messages that have the same symbol.
        Symbol symbol;
        var lockKey = $"{nameof(TradeConsumer)}:{marketId}:{context.Message.SymbolCode}";
        using (await _itemLock.LockAsync(lockKey))
        {
            var upsertSymbolRequest = new UpsertSymbolInput(
                marketId,
                context.Message.SymbolCode,
                context.Message.SymbolSubcode,
                context.Message.SymbolName,
                context.Message.AdditionalFields
            );

            symbol = await _mediator.Send(upsertSymbolRequest);
        }

        var insertTradeRequest = new InsertTradeInput(
            marketId,
            symbol.Id,
            symbol.Name,
            symbol.Code,
            symbol.Subcode,
            context.Message.Price,
            context.Message.Volume,
            context.Message.Timestamp,
            context.Message.AdditionalFields
        );
        var trade = await _mediator.Send(insertTradeRequest);

        _logger.LogInformation(
            "Successfully consumed trade message for trade '{tradeId}', symbol '{symbolId}', and market '{marketId}'.",
            trade.Id, symbol.Id, marketId);
    }
}