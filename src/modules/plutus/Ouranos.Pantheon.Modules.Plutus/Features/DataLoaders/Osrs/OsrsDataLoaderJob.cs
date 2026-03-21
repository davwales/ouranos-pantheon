using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.DataLoaders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Utilities;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

public sealed class OsrsDataLoaderJob
{
    private static readonly IEnumerable<TimeSpan> StaleDataRetryDelays =
        Enumerable.Repeat(TimeSpan.FromSeconds(5), 6);

    private const string PayToPlaySubcode = "p2p";
    private const string FreeToPlaySubcode = "f2p";

    private readonly ILogger<OsrsDataLoaderJob> _logger;
    private readonly IWikiClient _wikiClient;
    private readonly IQueueTradeMessages _queueTradeMessages;
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IOptions<OsrsDataLoaderOptions> _options;

    public OsrsDataLoaderJob(
        ILogger<OsrsDataLoaderJob> logger,
        IWikiClient wikiClient,
        IQueueTradeMessages queueTradeMessages,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        IOptions<OsrsDataLoaderOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(wikiClient);
        Guard.Against.Null(queueTradeMessages);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(options);

        _logger = logger;
        _wikiClient = wikiClient;
        _queueTradeMessages = queueTradeMessages;
        _dbContextFactory = dbContextFactory;
        _options = options;
    }

    [TickerFunction(nameof(OsrsDataLoaderJob), "0 */5 * * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken cancellationToken)
    {
        if (!_options.Value.IsEnabled)
        {
            return;
        }

        await Retry.ExecuteAsync(
            async ct =>
            {
                var lastProcessed = await LoadLastProcessed(ct);
                var (timestamp, prices) = await FetchPrices(ct);

                if (timestamp == lastProcessed)
                {
                    throw new InvalidOperationException($"OSRS data unchanged at {timestamp}.");
                }

                var messages = await BuildTradeMessages(timestamp, prices, ct);
                await PublishTrades(messages, timestamp, ct);
                await SaveLastProcessed(timestamp, ct);
            },
            StaleDataRetryDelays,
            cancellationToken
        );
    }

    private async Task<DateTimeOffset?> LoadLastProcessed(CancellationToken ct)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var state = await db.OsrsDataLoaderStates.FindAsync([OsrsDataLoaderState.SingletonId], ct);
        return state?.LastProcessed;
    }

    private async Task SaveLastProcessed(DateTimeOffset timestamp, CancellationToken ct)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var state = await db.OsrsDataLoaderStates.FindAsync([OsrsDataLoaderState.SingletonId], ct);
        state!.LastProcessed = timestamp;
        await db.SaveChangesAsync(ct);
    }

    private async Task<(DateTimeOffset timestamp, PriceResponse prices)> FetchPrices(CancellationToken ct)
    {
        var prices = await _wikiClient.GetPrices(ct);
        var timestamp = DateTimeOffset.FromUnixTimeSeconds(prices.Timestamp);
        return (timestamp, prices);
    }

    private async Task<List<TradeMessage>> BuildTradeMessages(
        DateTimeOffset timestamp,
        PriceResponse prices,
        CancellationToken ct
    )
    {
        var mappings = await _wikiClient.GetMappings(ct);
        return MapTrades(timestamp, mappings, prices.Data);
    }

    private async Task PublishTrades(List<TradeMessage> messages, DateTimeOffset timestamp, CancellationToken ct)
    {
        if (messages.Count > 0)
        {
            await _queueTradeMessages.QueueMessages(messages, ct);
            _logger.LogInformation("Processed {Count} OSRS trades for {Timestamp}.", messages.Count, timestamp);
        }
    }

    private List<TradeMessage> MapTrades(
        DateTimeOffset timestamp,
        List<Mapping> mappings,
        Dictionary<string, Price> prices
    )
    {
        List<TradeMessage> trades = [];
        var mappingLookup = mappings.ToDictionary(m => m.Id.ToString(), m => m);

        foreach (var (code, price) in prices)
        {
            if (!mappingLookup.TryGetValue(code, out var mapping))
            {
                _logger.LogWarning("Found price for item '{ItemCode}' with no mapping.", code);
                continue;
            }

            var subcode = mapping.IsMembers ? PayToPlaySubcode : FreeToPlaySubcode;
            var additionalFields = new AdditionalFields(mapping.Limit, mapping.HighAlch, mapping.LowAlch);

            if (price is { LowPriceVolume: > 0, AvgLowPrice: not null })
            {
                trades.Add(
                    new TradeMessage(
                        Producer.Osrs,
                        mapping.Id.ToString(),
                        subcode,
                        mapping.Name,
                        price.AvgLowPrice ?? 0,
                        price.LowPriceVolume,
                        timestamp,
                        additionalFields
                    )
                );
            }

            if (price is { HighPriceVolume: > 0, AvgHighPrice: not null })
            {
                trades.Add(
                    new TradeMessage(
                        Producer.Osrs,
                        mapping.Id.ToString(),
                        subcode,
                        mapping.Name,
                        price.AvgHighPrice ?? 0,
                        price.HighPriceVolume,
                        timestamp,
                        additionalFields
                    )
                );
            }
        }

        return trades;
    }
}
