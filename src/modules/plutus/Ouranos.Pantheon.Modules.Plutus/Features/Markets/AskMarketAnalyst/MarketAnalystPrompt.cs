using System.Globalization;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst;

internal static class MarketAnalystPrompt
{
    public const string Instructions = """
        You are a market analyst. Help the user decide what to trade using only the data below,
        which mirrors the page they are viewing. Quote the numbers you rely on and never invent
        symbols or prices. Margin, ROI and Gain are already after tax; Gain is Margin × the
        tradable volume. Rows with few trades (transactions) are unreliable, so say so. Recommend
        at most three symbols, each with a one-line reason. Signals, forecasts and positions are
        not shown here; point the user to the Signals, Forecasts or Portfolio page for those.
        This is not financial advice. Times are UTC.
        """;

    internal const int HeatmapSlots = 3;

    private static readonly string[] DayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public static string Compose(MarketAnalystSnapshot snapshot)
    {
        string[] sections =
        [
            Instructions,
            ComposeMarket(snapshot),
            ComposePulse(snapshot),
            ComposeActivity(snapshot),
            ComposeView(snapshot),
        ];

        return string.Join("\n\n", sections.Where(section => section.Length > 0));
    }

    private static string ComposeMarket(MarketAnalystSnapshot snapshot)
    {
        var flat = snapshot.Market.Taxes.Flat;

        var tax = flat is null
            ? "Tax: none."
            : $"Tax: {FormatPercent(flat.Rate)} of sell price "
                + $"(min {FormatCompact(flat.Minimum)}, max {FormatCompact(flat.Maximum)}).";

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
            $"Avg price {FormatCompact(overview.AveragePrice)}, "
            + $"volume {FormatCompact(overview.Volume)}, "
            + $"{FormatCompact(overview.NumTransactions)} trades.";

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

                    return $"{date} {FormatCompact(price)}/{FormatCompact(volume)}";
                }),
        ];
    }

    private static string ComposeActivity(MarketAnalystSnapshot snapshot)
    {
        var cells = snapshot.Heatmap.Rows.Where(cell => cell.TotalTrades > 0).ToList();

        if (cells.Count == 0)
        {
            return "";
        }

        var busiest = cells
            .OrderByDescending(cell => cell.TotalTrades)
            .Take(HeatmapSlots)
            .Select(cell =>
                $"{DayNames[cell.DayOfWeek % 7]} {cell.Hour:00}:00 ({FormatNumber(cell.Percentage)}%)"
            );

        var quietest = cells
            .OrderBy(cell => cell.TotalTrades)
            .Take(HeatmapSlots)
            .Select(cell =>
                $"{DayNames[cell.DayOfWeek % 7]} {cell.Hour:00}:00 ({FormatNumber(cell.Percentage)}%)"
            );

        return $"""
            ## Activity (last {MarketAnalyst.HeatmapLookbackWeeks} weeks, share of trades)
            Busiest: {string.Join(", ", busiest)}
            Quietest: {string.Join(", ", quietest)}
            """;
    }

    private static string ComposeView(MarketAnalystSnapshot snapshot)
    {
        var view = snapshot.View;
        var context = snapshot.Context;
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

        var rows = items.Select(row =>
            string.Join(
                " | ",
                FormatName(row),
                FormatCompact(row.MinPrice),
                FormatCompact(row.MaxPrice),
                row.TotalVolume == 0 ? "-" : FormatCompact(row.AveragePrice),
                FormatCompact(row.TotalVolume),
                FormatCompact(row.NumTransactions),
                FormatCompact(row.Margin),
                row.MinPrice == 0 ? "-" : FormatPercent(row.Roi),
                FormatCompact(row.TotalGain)
            )
        );

        return $"""
            ## Rows in view ({first}-{last} of {view.TotalCount}; sorted by {sort}{filter})
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

    internal static string FormatCompact(decimal value)
    {
        var magnitude = Math.Abs(value);

        return magnitude switch
        {
            >= 1_000_000_000m => FormatNumber(value / 1_000_000_000m, "0.#") + "B",
            >= 1_000_000m => FormatNumber(value / 1_000_000m, "0.#") + "M",
            >= 10_000m => FormatNumber(value / 1_000m, "0.#") + "K",
            _ => FormatNumber(value),
        };
    }

    private static string FormatPercent(decimal ratio)
    {
        return FormatNumber(ratio * 100, "0.#") + "%";
    }

    private static string FormatNumber(decimal value, string format = "0.##")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }
}
