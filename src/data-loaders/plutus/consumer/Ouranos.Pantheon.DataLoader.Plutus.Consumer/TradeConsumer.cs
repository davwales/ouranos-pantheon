using Ardalis.GuardClauses;
using MassTransit;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.CheckDuplication;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.UpsertSymbol;
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

        if (context.MessageId is not null)
        {
            var checkDuplicationInput = new CheckDuplicationInput(context.MessageId.Value);
            var checkDuplicationResponse = await _dispatcher.Send(checkDuplicationInput, context.CancellationToken);

            if (checkDuplicationResponse.IsDuplicate)
            {
                _logger.LogInformation("Skipping message '{messageId}' because it is a duplicate.", context.MessageId);
                return;
            }
        }

        var upsertSymbolRequest = new UpsertSymbolInput(
            marketId,
            context.Message.SymbolCode,
            context.Message.SymbolSubcode,
            context.Message.SymbolName,
            context.Message.AdditionalFields
        );

        var symbol = await _dispatcher.Send(upsertSymbolRequest, context.CancellationToken);

        var insertTradeRequest = new InsertTradeInput(
            marketId,
            symbol.Id,
            symbol.Name,
            symbol.Code,
            symbol.Subcode,
            context.Message.Price,
            context.Message.Volume,
            context.Message.Timestamp,
            context.Message.AdditionalFields,
            context.MessageId
        );
        var trade = await _dispatcher.Send(insertTradeRequest);

        _logger.LogInformation(
            "Successfully consumed trade message '{messageId}' for trade '{tradeId}', symbol '{symbolId}', and market '{marketId}'.",
            context.MessageId, trade.Id, symbol.Id, marketId);
    }
}