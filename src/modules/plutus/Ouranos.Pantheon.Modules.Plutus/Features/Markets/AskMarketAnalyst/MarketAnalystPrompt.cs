using System.Globalization;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst;

internal static class MarketAnalystPrompt
{
    public const string Instructions = """
        You are a market analyst. Help the user decide what to trade using only the data below,
        which mirrors the page they are viewing. Quote the numbers you rely on and never invent
        symbols or prices. Margin, ROI and Gain are already after tax; Gain is Margin × the
        tradable volume. Rows with few trades (transactions) are unreliable, so say so. Recommend
        at most three symbols, each with a one-line reason. Tools can screen the whole market,
        look up one symbol, rank symbols by signal or forecast, and show the busiest trading
        hours. When the user asks to show, filter or sort the rows they see, propose the view
        once with the view tool instead of listing matches. ROI values are percentages. This is not
        financial advice. Times are UTC.
        """;

    public static string Compose(MarketAnalystSnapshot snapshot)
    {
        string[] sections =
        [
            Instructions,
            ComposeMarket(snapshot),
            ComposePulse(snapshot),
            ComposeView(snapshot),
        ];

        return string.Join("\n\n", sections.Where(section => section.Length > 0));
    }

    private static string ComposeMarket(MarketAnalystSnapshot snapshot)
    {
        var flat = snapshot.Market.Taxes.Flat;

        var tax = flat is null
            ? "Tax: none."
            : $"Tax: {AssistantNumberFormat.Percent(flat.Rate)} of sell price "
                + $"(min {AssistantNumberFormat.Compact(flat.Minimum)}, max {AssistantNumberFormat.Compact(flat.Maximum)}).";

        return $"""
            ## Market: {snapshot.Market.Name}
            {tax}
            Data as of {snapshot.GeneratedAt.UtcDateTime.ToString(
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture
            )}.
            """;
    }

    private static string ComposePulse(MarketAnalystSnapshot snapshot)
    {
        var overview = snapshot.Overview;
        var summary =
            $"Avg price {AssistantNumberFormat.Compact(overview.AveragePrice)}, "
            + $"volume {AssistantNumberFormat.Compact(overview.Volume)}, "
            + $"{AssistantNumberFormat.Compact(overview.NumTransactions)} trades.";

        var trend = Downsample(overview.Trades, snapshot.TrendPoints);
        var trendLine =
            trend.Count == 0
                ? "No trades in this period."
                : "Trend (time avg/vol): " + string.Join(", ", trend);

        return $"""
            ## Pulse ({snapshot.TimeFrame})
            {summary}
            {trendLine}
            """;
    }

    private static List<string> Downsample(
        List<GetMarketOverviewBucketResponse> buckets,
        int points
    )
    {
        if (buckets.Count == 0 || points <= 0)
        {
            return [];
        }

        var ordered = buckets.OrderBy(bucket => bucket.Date).ToList();
        var size = (int)Math.Ceiling(ordered.Count / (double)points);

        return
        [
            .. ordered
                .Chunk(size)
                .Select(chunk =>
                {
                    var volume = chunk.Sum(bucket => bucket.Volume);
                    var price =
                        volume == 0
                            ? chunk.Average(bucket => bucket.Price)
                            : chunk.Sum(bucket => bucket.TotalSpent) / volume;
                    var date = chunk[0]
                        .Date.UtcDateTime.ToString("MM-dd HH:mm", CultureInfo.InvariantCulture);

                    return $"{date} {AssistantNumberFormat.Compact(price)}/{AssistantNumberFormat.Compact(volume)}";
                }),
        ];
    }

    private static string ComposeView(MarketAnalystSnapshot snapshot)
    {
        if (snapshot.View is not { } view || snapshot.Context.View is not { } context)
        {
            return "";
        }

        var items = view.Items.ToList();

        if (items.Count == 0)
        {
            return """
                ## Rows in view
                No symbols match the user's current view.
                """;
        }

        var first = context.Skip + 1;
        var last = context.Skip + items.Count;

        var sort = context.SortField is null
            ? "Gain desc"
            : $"{context.SortField} {context.SortDirection ?? "desc"}";

        var filter = context.Filter is { Length: > 0 } f
            ? $"; filter {string.Join(" and ", f)}"
            : "";

        return $"""
            ## Rows in view ({first}-{last} of {view.TotalCount}; sorted by {sort}{filter})
            {ComposeRows(items)}
            """;
    }

    private static string ComposeRows(IEnumerable<GetMarketTradesResponse> items)
    {
        var rows = items.Select(row =>
            string.Join(
                " | ",
                FormatName(row),
                AssistantNumberFormat.Compact(row.MinPrice),
                AssistantNumberFormat.Compact(row.MaxPrice),
                row.TotalVolume == 0 ? "-" : AssistantNumberFormat.Compact(row.AveragePrice),
                AssistantNumberFormat.Compact(row.TotalVolume),
                AssistantNumberFormat.Compact(row.NumTransactions),
                AssistantNumberFormat.Compact(row.Margin),
                row.MinPrice == 0 ? "-" : AssistantNumberFormat.Percent(row.Roi),
                AssistantNumberFormat.Compact(row.TotalGain)
            )
        );

        return $"""
            Name | Min | Max | Avg | Vol | Txns | Margin | ROI | Gain
            {string.Join("\n", rows)}
            """;
    }

    private static string FormatName(GetMarketTradesResponse row)
    {
        return string.IsNullOrWhiteSpace(row.SymbolSubcode)
            ? row.SymbolName
            : $"{row.SymbolName} ({row.SymbolSubcode})";
    }
}
