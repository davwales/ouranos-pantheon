using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Ouranos.Pantheon.DataLoader.Plutus.Domain;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.ConvertTrade;

public sealed class ConvertTradeAction : IConvertTradeAction
{
    private readonly ILogger<ConvertTradeAction> _logger;
    private readonly Dictionary<ObjectId, Producer> _producerMap;

    public ConvertTradeAction(
        ILogger<ConvertTradeAction> logger,
        IConfiguration configuration
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(configuration);

        _logger = logger;
        _producerMap = configuration
                           .GetSection("Ouranos:Markets")
                           .Get<Dictionary<string, Producer>>()
                           ?.ToDictionary(x => new ObjectId(x.Key), x => x.Value)
                       ?? throw new InvalidOperationException("Cannot find market map in configuration.");
    }

    public TradeMessage? ConvertTrade(TalosTrade? trade)
    {
        _logger.LogTrace("Attempting to convert trade '{@trade}'.", trade);

        if (trade?.MetaData?.Symbol is null || string.IsNullOrWhiteSpace(trade.MetaData.Symbol.Code))
        {
            _logger.LogError("Skipping invalid trade.");
            return null;
        }

        if (!_producerMap.TryGetValue(trade.MetaData.Symbol.MarketId, out var producer))
        {
            _logger.LogError("Failed to find valid producer for market '{marketId}'.",
                trade.MetaData.Symbol.MarketId);
            return null;
        }

        var message = new TradeMessage(
            producer,
            trade.MetaData.Symbol.Code,
            trade.MetaData.Symbol.Subcode,
            trade.MetaData.Symbol.Name,
            trade.Price,
            trade.Volume,
            trade.Date,
            new AdditionalFields(
                trade.MetaData.Symbol.AdditionalFields?.Limit,
                trade.MetaData.Symbol.AdditionalFields?.HighAlch,
                trade.MetaData.Symbol.AdditionalFields?.LowAlch
            )
        );

        _logger.LogDebug("Successfully converted trade.");
        return message;
    }
}