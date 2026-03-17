using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

public sealed class OsrsWorker : BackgroundService
{
    private readonly ILogger<OsrsWorker> _logger;
    private readonly IWikiClient _wikiClient;
    private readonly IQueueTradeMessages _queueTradeMessages;
    private readonly TimeSpan _interval;
    private DateTimeOffset? _lastRefresh;

    public OsrsWorker(
        ILogger<OsrsWorker> logger,
        IWikiClient wikiClient,
        IQueueTradeMessages queueTradeMessages,
        IOptions<OsrsDataLoaderOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(wikiClient);
        Guard.Against.Null(queueTradeMessages);
        Guard.Against.Null(options);

        _logger = logger;
        _wikiClient = wikiClient;
        _queueTradeMessages = queueTradeMessages;
        _interval = TimeSpan.FromMinutes(options.Value.RefreshIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OSRS worker starting.");

        using var timer = new PeriodicTimer(_interval);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTrades(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occurred during OSRS worker execution.");
            }

            await timer.WaitForNextTickAsync(cancellationToken);
        }

        _logger.LogInformation("Cancellation requested, OSRS worker stopped.");
    }

    private async Task ProcessTrades(CancellationToken cancellationToken)
    {
        var prices = await _wikiClient.GetPrices(cancellationToken);
        var timestamp = DateTimeOffset.FromUnixTimeSeconds(prices.Timestamp);

        if (_lastRefresh is null)
        {
            _lastRefresh = timestamp;
            _logger.LogDebug("OSRS worker initialized with timestamp {timestamp}.", timestamp);
            return;
        }

        if (timestamp == _lastRefresh)
        {
            _logger.LogDebug("OSRS prices unchanged, skipping.");
            return;
        }

        var mappings = await _wikiClient.GetMappings(cancellationToken);
        var messages = MapTrades(timestamp, mappings, prices.Data);

        if (messages.Count > 0)
        {
            await _queueTradeMessages.QueueMessages(messages, cancellationToken);
            _logger.LogInformation("Successfully processed '{messageCount}' OSRS trades.", messages.Count);
        }

        _lastRefresh = timestamp;
    }

    private List<TradeMessage> MapTrades(
        DateTimeOffset timestamp,
        List<Mapping> mappings,
        Dictionary<string, Price> prices
    )
    {
        List<TradeMessage> trades = [];
        var mappingLookup = mappings.ToDictionary(m => m.Id.ToString(), m => m);

        const string payToPlaySubcode = "p2p";
        const string freeToPlaySubcode = "f2p";

        foreach (var (code, price) in prices)
        {
            if (!mappingLookup.TryGetValue(code, out var mapping))
            {
                _logger.LogWarning("Found price for item '{itemCode}' with no mapping.", code);
                continue;
            }

            var subcode = mapping.IsMembers ? payToPlaySubcode : freeToPlaySubcode;
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
