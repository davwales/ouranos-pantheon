using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory;

public sealed class GetPriceHistoryTool(
    ILogger<GetPriceHistoryTool> logger,
    IMessageBus bus,
    IOptions<PlutusOptions> options
) : AssistantDataTool<ISymbolScope, PriceHistoryArguments, PriceHistoryData>(logger)
{
    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    public override string Name => "get_price_history";

    public override string Description =>
        "Call this for the latest trade, after-tax margin and ROI, price trends or any period "
        + "beyond today. Returns, for one time frame, the latest trade, the price summary with "
        + "margin and ROI, the percent change, and a chart of downsampled buckets.";

    protected override async ValueTask<AssistantData<PriceHistoryData>> QueryAsync(
        ISymbolScope context,
        PriceHistoryArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var chartPoints = _options.Value.AssistantTools.ChartPoints;
        var trades = _bus.InvokeAsync<GetSymbolTradesResponse>(
            new GetSymbolTradesInput(context.SymbolId, arguments.TimeFrame, chartPoints),
            cancellationToken
        );
        var latestTrades = _bus.InvokeAsync<List<GetAllTradesResponse>>(
            new GetAllTradesInput(
                TimeFrame.OneYear,
                nameof(GetAllTradesResponse.Timestamp),
                SortDirection.Desc.ToQueryValue(),
                Take: 1,
                Filter: AssistantToolQueries.SymbolFilter(context.SymbolId)
            ),
            cancellationToken
        );

        var summary = await AssistantToolQueries.LoadPriceSummaryAsync(
            _bus,
            context.MarketId,
            arguments.TimeFrame,
            context.SymbolId,
            cancellationToken
        );

        var buckets = (await trades).Trades.OrderBy(bucket => bucket.Date).ToList();

        return new PriceHistoryData(
            LatestTrade((await latestTrades).FirstOrDefault()),
            summary,
            ChangePercent(buckets),
            [
                .. Downsample(buckets, chartPoints)
                    .Select(bucket => new PriceBucketData(
                        bucket.Date,
                        bucket.Price,
                        bucket.MinPrice,
                        bucket.MaxPrice,
                        bucket.Volume
                    )),
            ]
        );
    }

    private static LatestTradeData? LatestTrade(GetAllTradesResponse? trade)
    {
        return trade is null
            ? null
            : new LatestTradeData(trade.Price, trade.Volume, trade.Timestamp);
    }

    private static decimal? ChangePercent(List<GetSymbolTradeBucketsResponse> buckets)
    {
        if (buckets.Count == 0 || buckets[0].OpenPrice <= 0)
        {
            return null;
        }

        var open = buckets[0].OpenPrice;
        return AssistantToolData.Percent((buckets[^1].ClosePrice - open) / open);
    }

    internal static List<GetSymbolTradeBucketsResponse> Downsample(
        List<GetSymbolTradeBucketsResponse> ordered,
        int points
    )
    {
        if (ordered.Count <= points || points <= 0)
        {
            return ordered;
        }

        var size = (int)Math.Ceiling(ordered.Count / (double)points);

        return
        [
            .. ordered
                .Chunk(size)
                .Select(chunk =>
                {
                    var traded = chunk.Where(bucket => bucket.Volume > 0).ToArray();
                    var priced = traded.Length > 0 ? traded : chunk;
                    var volume = chunk.Sum(bucket => bucket.Volume);
                    var totalSpent = chunk.Sum(bucket => bucket.TotalSpent);

                    return new GetSymbolTradeBucketsResponse(
                        Price: volume == 0
                            ? chunk.Average(bucket => bucket.Price)
                            : totalSpent / volume,
                        Volume: volume,
                        TotalSpent: totalSpent,
                        MinPrice: priced.Min(bucket => bucket.MinPrice),
                        MaxPrice: priced.Max(bucket => bucket.MaxPrice),
                        NumTransactions: chunk.Sum(bucket => bucket.NumTransactions),
                        Date: chunk[0].Date,
                        OpenPrice: priced[0].OpenPrice,
                        ClosePrice: priced[^1].ClosePrice
                    );
                }),
        ];
    }
}
