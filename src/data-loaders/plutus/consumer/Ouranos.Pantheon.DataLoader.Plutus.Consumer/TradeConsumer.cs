using Ardalis.GuardClauses;
using MassTransit;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Messages;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer;

public sealed class TradeConsumer : IConsumer<TradeMessage>
{
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<TradeConsumer> _logger;
    private readonly Dictionary<Producer, Id<Market>> _marketMap;

    public TradeConsumer(
        ILogger<TradeConsumer> logger,
        IDispatcher dispatcher,
        IConfiguration configuration
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dispatcher);
        Guard.Against.Null(configuration);

        _logger = logger;
        _dispatcher = dispatcher;
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

        var upsertSymbolRequest = new UpsertSymbolMessage(
            marketId,
            context.Message.SymbolCode,
            context.Message.SymbolSubcode,
            context.Message.SymbolName,
            context.Message.AdditionalFields
        );

        var symbol = await _dispatcher.Send(upsertSymbolRequest, context.CancellationToken);

        var insertTradeRequest = new InsertTradeMessage(
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
        var trade = await _dispatcher.Send(insertTradeRequest);

        _logger.LogInformation(
            "Successfully consumed trade message for trade '{tradeId}', symbol '{symbolId}', and market '{marketId}'.",
            trade.Id, symbol.Id, marketId);
    }
}