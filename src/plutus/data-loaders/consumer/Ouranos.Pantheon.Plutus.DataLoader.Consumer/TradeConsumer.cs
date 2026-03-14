using Ardalis.GuardClauses;
using MassTransit;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.InsertTrade;
using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.UpsertSymbol;
using Ouranos.Pantheon.Plutus.DataLoader.Domain;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using LegacyNamespace = Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using TradeMessage = Ouranos.Pantheon.Plutus.DataLoader.Domain.Trades.TradeMessage;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer;

public sealed class TradeConsumer : IConsumer<TradeMessage>, IConsumer<LegacyNamespace.TradeMessage>
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
        await Process(context.MessageId, context.Message, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<LegacyNamespace.TradeMessage> context)
    {
        _logger.LogDebug("Detected legacy trade message '{messageId}', converting and processing.", context.MessageId);

        await Process(
            context.MessageId,
            new TradeMessage(
                context.Message.Producer,
                context.Message.SymbolCode,
                context.Message.SymbolSubcode,
                context.Message.SymbolName,
                context.Message.Price,
                context.Message.Volume,
                context.Message.Timestamp,
                context.Message.AdditionalFields
            ),
            context.CancellationToken
        );
    }

    private async Task Process(Guid? messageId, TradeMessage message, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Attempting to consume trade message '{@messageId}'.", messageId);

        if (!_marketMap.TryGetValue(message.Producer, out var marketId))
        {
            throw new InvalidOperationException("Cannot find market for this message.");
        }

        var symbol = await _upsertSymbol.UpsertSymbolAsync(
            new UpsertSymbolInput(
                marketId,
                message.SymbolCode,
                message.SymbolSubcode,
                message.SymbolName,
                message.AdditionalFields
            ),
            cancellationToken
        );

        var trade = await _insertTrade.InsertTradeAsync(
            new InsertTradeInput(
                symbol,
                message.Price,
                message.Volume,
                message.Timestamp,
                messageId
            ),
            cancellationToken
        );

        _logger.LogInformation(
            "Successfully consumed trade message '{messageId}' for trade '{tradeId}', symbol '{symbolId}', and market '{marketId}'.",
            messageId,
            trade.Id,
            symbol.Id,
            marketId
        );
    }
}