using Ardalis.GuardClauses;
using MassTransit;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.UpsertSymbol;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer;

public sealed class TradeConsumer : IConsumer<TradeMessage>
{
    private readonly IInsertTrade _insertTrade;
    private readonly ILogger<TradeConsumer> _logger;
    private readonly Dictionary<Producer, Id<Market>> _marketMap;
    private readonly IUpsertSymbol _upsertSymbol;

    public TradeConsumer(
        ILogger<TradeConsumer> logger,
        IUpsertSymbol upsertSymbol,
        IInsertTrade insertTrade,
        IConfiguration configuration
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(upsertSymbol);
        Guard.Against.Null(insertTrade);
        Guard.Against.Null(configuration);

        _logger = logger;
        _upsertSymbol = upsertSymbol;
        _insertTrade = insertTrade;
        _marketMap = configuration
            .GetSection("Ouranos:Markets")
            .Get<Dictionary<Producer, string>>()
            ?.ToDictionary(
                x => x.Key,
                x => new Id<Market>(x.Value)
            ) ?? throw new InvalidOperationException("Cannot find market map in configuration.");
    }

    public async Task Consume(ConsumeContext<TradeMessage> context)
    {
        _logger.LogTrace("Attempting to consume trade message '{@message}'.", context.Message);

        if (!_marketMap.TryGetValue(context.Message.Producer, out var marketId))
        {
            throw new InvalidOperationException("Cannot find market for this message.");
        }

        var upsertSymbolRequest = new UpsertSymbolInput(
            marketId,
            context.Message.SymbolCode,
            context.Message.SymbolSubcode,
            context.Message.SymbolName,
            context.Message.AdditionalFields
        );
        var symbol = await _upsertSymbol.UpsertSymbolAsync(upsertSymbolRequest, context.CancellationToken);

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
        var trade = await _insertTrade.InsertTradeAsync(insertTradeRequest, context.CancellationToken);

        _logger.LogInformation(
            "Successfully consumed trade message '{messageId}' for trade '{tradeId}', symbol '{symbolId}', and market '{marketId}'.",
            context.MessageId, trade.Id, symbol.Id, marketId);
    }
}