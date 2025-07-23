using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki.Trades;

public sealed class GetTrades : IGetTrades
{
    private readonly ILogger<GetTrades> _logger;
    private readonly IOptions<OsrsWikiOptions> _options;
    private readonly IWikiClient _wikiClient;
    private DateTimeOffset? _lastRefresh;

    public GetTrades(
        ILogger<GetTrades> logger,
        IWikiClient wikiClient,
        IOptions<OsrsWikiOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(wikiClient);
        Guard.Against.Null(options);
        Guard.Against.Null(options);

        _logger = logger;
        _wikiClient = wikiClient;
        _options = options;
    }

    public async Task<List<GetTradesResponse>> GetTradesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to get trades from the osrs wiki.");
        cancellationToken.ThrowIfCancellationRequested();

        if (_lastRefresh is null)
        {
            var initialPrices = await _wikiClient.GetPrices(cancellationToken);
            var initialTimestamp = ParseTimestamp(initialPrices.Timestamp);
            _lastRefresh = initialTimestamp;
            return [];
        }

        var timeSinceRefresh = DateTimeOffset.UtcNow - _lastRefresh;
        if (timeSinceRefresh < TimeSpan.FromMinutes(_options.Value.RefreshIntervalMinutes))
        {
            return [];
        }

        var prices = await _wikiClient.GetPrices(cancellationToken);
        var timestamp = ParseTimestamp(prices.Timestamp);
        if (timestamp == _lastRefresh)
        {
            return [];
        }

        var mappings = await _wikiClient.GetMappings(cancellationToken);
        var validTrades = MapTrades(timestamp, mappings, prices.Data);
        _lastRefresh = ParseTimestamp(prices.Timestamp);

        _logger.LogDebug("Successfully found '{tradeCount}' trades from the osrs wiki.", validTrades.Count);
        return validTrades;
    }

    private List<GetTradesResponse> MapTrades(
        DateTimeOffset timestamp,
        List<Mapping> mappings,
        Dictionary<string, Price> prices
    )
    {
        List<GetTradesResponse> trades = [];
        var mappingLookup = mappings.ToDictionary(m => m.Id.ToString(), m => m);
        foreach (var (code, price) in prices)
        {
            if (!mappingLookup.TryGetValue(code, out var mapping))
            {
                _logger.LogWarning("Found price for item '{itemCode}' with no mapping.", code);
                continue;
            }

            if (price is { LowPriceVolume: > 0, AvgLowPrice: not null })
            {
                var lowPriceResponse = CreateResponse(mapping, price.AvgLowPrice ?? 0, price.LowPriceVolume, timestamp);
                trades.Add(lowPriceResponse);
            }

            if (price is { HighPriceVolume: > 0, AvgHighPrice: not null })
            {
                var highPriceResponse =
                    CreateResponse(mapping, price.AvgHighPrice ?? 0, price.HighPriceVolume, timestamp);
                trades.Add(highPriceResponse);
            }
        }

        return trades;
    }

    private static DateTimeOffset ParseTimestamp(int timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp);
    }

    private static GetTradesResponse CreateResponse(
        Mapping mapping,
        int price,
        int volume,
        DateTimeOffset timestamp
    )
    {
        const string payToPlaySubcode = "p2p";
        const string freeToPlaySubcode = "f2p";

        return new GetTradesResponse(
            mapping.Id.ToString(),
            mapping.IsMembers ? payToPlaySubcode : freeToPlaySubcode,
            mapping.Name,
            price,
            volume,
            new GetTradesAdditionalFieldsResponse(mapping.LowAlch, mapping.HighAlch, mapping.Limit),
            timestamp
        );
    }
}