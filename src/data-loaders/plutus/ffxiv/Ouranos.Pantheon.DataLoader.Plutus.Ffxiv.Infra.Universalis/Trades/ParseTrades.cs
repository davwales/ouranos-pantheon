using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Dtos;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Trades;

public sealed class ParseTrades : IParseTrades
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<ParseTrades> _logger;

    public ParseTrades(ILogger<ParseTrades> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task<List<TradeDto>> ParseTradeMessage(
        byte[] message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to parse trade from Universalis message.");
        cancellationToken.ThrowIfCancellationRequested();

        var parsedMessage = BsonSerializer.Deserialize<Message>(message);
        var trades = parsedMessage?.Sales.Select(sale => new TradeDto(
            parsedMessage.Item.ToString(),
            sale.Hq,
            sale.PricePerUnit,
            sale.Quantity,
            DateTimeOffset.FromUnixTimeSeconds(sale.Timestamp)
        )).ToList() ?? [];

        _logger.LogTrace("Successfully parsed '{tradeCount}' Universalis trades.", trades.Count);
        return await Task.FromResult(trades);
    }
}