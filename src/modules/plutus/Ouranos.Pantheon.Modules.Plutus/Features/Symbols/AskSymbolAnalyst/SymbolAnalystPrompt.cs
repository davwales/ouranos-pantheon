using System.Globalization;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

internal static class SymbolAnalystPrompt
{
    public const string Instructions = """
        You are a trading analyst for the single symbol below. Use only this data and quote the
        numbers you rely on; never invent prices or trades, and say so when data is missing or
        stale. Margin and ROI are after tax; apply the tax to any sell price you suggest.
        Intents: Buy and Sell are directional, Flip trades the low-high spread, Merch holds for a
        larger move. Signals range from -1 (bearish) to 1 (bullish). Weigh forecasts by their
        error. Be concise, give concrete price levels, and note the uncertainty and that this is
        not financial advice. K, M, B, and T mean thousand, million, billion, and trillion.
        Times are UTC.
        """;

    internal const int MaxNoteLength = 40;

    internal const int MaxDescriptionLength = 80;

    private static readonly int[] ForecastDays = [1, 3, 7];

    public static string Compose(SymbolAnalystSnapshot snapshot)
    {
        string[] sections =
        [
            Instructions,
            ComposeSymbol(snapshot),
            ComposeWindows(snapshot),
            ComposeChart(snapshot),
            ComposeSignals(snapshot),
            ComposeForecast(snapshot),
            ComposePositions(snapshot),
        ];

        return string.Join("\n\n", sections.Where(section => section.Length > 0));
    }

    private static string ComposeSymbol(SymbolAnalystSnapshot snapshot)
    {
        var symbol = snapshot.Symbol;
        var fields = symbol.AdditionalFields;

        var code = string.IsNullOrWhiteSpace(symbol.Subcode)
            ? symbol.Code
            : $"{symbol.Code} ({symbol.Subcode})";

        var market = string.IsNullOrWhiteSpace(snapshot.Market.Description)
            ? snapshot.Market.Name
            : $"{snapshot.Market.Name} ({Truncate(snapshot.Market.Description, MaxDescriptionLength)})";

        var lines = new List<string> { $"- Code: {code}", $"- Market: {market}" };

        if (fields.Limit is { } limit)
        {
            lines.Add($"- Buy limit: {Compact(limit)}");
        }

        if (FormatAlchemy(fields) is { } alchemy)
        {
            lines.Add(alchemy);
        }

        if (!string.IsNullOrWhiteSpace(fields.Exchange))
        {
            lines.Add($"- Exchange: {fields.Exchange}");
        }

        lines.Add(FormatTax(snapshot));
        lines.Add(FormatLatestTrade(snapshot));
        lines.Add($"- Data as of: {FormatDateTime(snapshot.GeneratedAt)}");

        return $"""
            ## Symbol: {symbol.Name}
            {string.Join("\n", lines)}
            """;
    }

    private static string? FormatAlchemy(AdditionalFields fields)
    {
        var values = new List<string>();

        if (fields.HighAlch is { } high)
        {
            values.Add($"high {Compact(high)}");
        }

        if (fields.LowAlch is { } low)
        {
            values.Add($"low {Compact(low)}");
        }

        return values.Count == 0
            ? null
            : $"- Alchemy values (fixed NPC sale prices, not market prices): {string.Join(", ", values)}";
    }

    private static string FormatTax(SymbolAnalystSnapshot snapshot)
    {
        var flat = snapshot.Market.Taxes.Flat;

        return flat is null
            ? "- Tax: none"
            : $"- Tax: {Percent(flat.Rate)} of the sell price per item "
                + $"(min {Compact(flat.Minimum)}, max {Compact(flat.Maximum)})";
    }

    private static string FormatLatestTrade(SymbolAnalystSnapshot snapshot)
    {
        var trade = snapshot.LatestTrade;

        return trade is null
            ? "- Latest trade: none in the last year"
            : $"- Latest trade: {Compact(trade.Price)} × {Compact(trade.Volume)} "
                + $"at {FormatDateTime(trade.Timestamp)}";
    }

    private static string ComposeWindows(SymbolAnalystSnapshot snapshot)
    {
        var rows = snapshot.Windows.Select(window =>
            window.Trades is { TotalVolume: > 0 } trades
                ? FormatWindow(window.TimeFrame, trades)
                : $"{window.TimeFrame} | no trades"
        );

        return $"""
            ## Price Summary (Margin = high - low - tax; ROI = margin / low)
            Window | Avg | Low | High | Vol | Margin | ROI
            {string.Join("\n", rows)}
            """;
    }

    private static string FormatWindow(TimeFrame timeFrame, GetMarketTradesResponse trades)
    {
        return string.Join(
            " | ",
            timeFrame,
            Compact(trades.AveragePrice),
            Compact(trades.MinPrice),
            Compact(trades.MaxPrice),
            Compact(trades.TotalVolume),
            Compact(trades.Margin),
            trades.MinPrice > 0 ? Percent(trades.Roi) : "-"
        );
    }

    private static string ComposeChart(SymbolAnalystSnapshot snapshot)
    {
        var trades = snapshot.SelectedTrades;
        var heading = $"## {snapshot.TimeFrame} Chart (the period the user is viewing)";

        if (trades.Trades.Count == 0)
        {
            return $"""
                {heading}
                No trades in this period.
                """;
        }

        var ordered = trades.Trades.OrderBy(bucket => bucket.Date).ToList();
        var open = ordered[0].OpenPrice;
        var close = ordered[^1].ClosePrice;
        var change = open > 0 ? $" ({FormatChange((close - open) / open)})" : "";
        var rows = Downsample(ordered, snapshot.ChartPoints)
            .Select(point => FormatChartPoint(point, snapshot.TimeFrame));

        return $"""
            {heading}
            Avg {Compact(trades.AveragePrice)}, low {Compact(trades.MinPrice)}, high {Compact(
                trades.MaxPrice
            )}, vol {Compact(trades.Volume)}, {Compact(
                trades.NumTransactions
            )} trades; open {Compact(open)} → close {Compact(close)}{change}.
            Start | Avg | Low | High | Vol
            {string.Join("\n", rows)}
            """;
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

    private static string FormatChartPoint(GetSymbolTradeBucketsResponse point, TimeFrame timeFrame)
    {
        var format = timeFrame >= TimeFrame.SixMonths ? "yyyy-MM-dd" : "MM-dd HH:mm";

        return string.Join(
            " | ",
            point.Date.UtcDateTime.ToString(format, CultureInfo.InvariantCulture),
            Compact(point.Price),
            Compact(point.MinPrice),
            Compact(point.MaxPrice),
            Compact(point.Volume)
        );
    }

    private static string ComposeSignals(SymbolAnalystSnapshot snapshot)
    {
        var signals = snapshot.Signals;

        if (signals.Signals.Count == 0)
        {
            return """
                ## Signals
                No signals have been computed for this symbol.
                """;
        }

        var summary = signals.Summary;

        var history = snapshot
            .SignalHistory.Signals.GroupBy(signal => signal.Type)
            .ToDictionary(group => group.Key, group => group.First());

        var lines = signals.Signals.Select(signal =>
        {
            var trend = history.TryGetValue(signal.Type, out var past) ? FormatTrend(past) : "";

            return $"- {signal.Label}: {Number(signal.Value)} {signal.Direction} {signal.Strength} "
                + $"({string.Join(", ", signal.Intents)}){trend}";
        });

        return $"""
            ## Signals (7d trend: oldest → latest daily value)
            Score {Number(summary.AggregatedScore)} ({summary.BullishCount} bullish, {summary.BearishCount} bearish, {summary.NeutralCount} neutral); flip favourable: {FormatBool(
                summary.IsFlipFavourable
            )}; merch favourable: {FormatBool(summary.IsMerchFavourable)}.
            {string.Join("\n", lines)}
            """;
    }

    private static string FormatTrend(SignalHistoryResponse signal)
    {
        var daily = signal
            .History.GroupBy(point => point.ComputedAt.UtcDateTime.Date)
            .OrderBy(day => day.Key)
            .Select(day => day.OrderBy(point => point.ComputedAt).Last().Value)
            .ToList();

        if (daily.Count < 2)
        {
            return "";
        }

        int[] picks = [0, daily.Count / 2, daily.Count - 1];
        var values = picks.Distinct().Select(i => Number(daily[i]));

        return $"; 7d {string.Join(" → ", values)}";
    }

    private static string ComposeForecast(SymbolAnalystSnapshot snapshot)
    {
        if (!snapshot.Market.IsForecastingEnabled)
        {
            return """
                ## Forecast
                Forecasting is disabled for this market.
                """;
        }

        var forecast = snapshot.Forecast;
        if (forecast is null)
        {
            return """
                ## Forecast
                No forecast is available for this symbol.
                """;
        }

        GetMarketForecastPredictionResponse[] days =
        [
            forecast.DayOne,
            forecast.DayTwo,
            forecast.DayThree,
            forecast.DayFour,
            forecast.DayFive,
            forecast.DaySix,
            forecast.DaySeven,
        ];

        var rows = ForecastDays.Select(day =>
            string.Join(
                " | ",
                day,
                Compact(days[day - 1].AveragePrice),
                Compact(days[day - 1].MinPrice),
                Compact(days[day - 1].MaxPrice),
                Compact(days[day - 1].Margin)
            )
        );

        var latest = forecast.Latest;
        var accuracy = FormatAccuracy(snapshot.ForecastEfficacy);

        return $"""
            ## Forecast (baseline day: avg {Compact(latest.AveragePrice)}, low {Compact(
                latest.MinPrice
            )}, high {Compact(latest.MaxPrice)}, vol {Compact(latest.Volume)})
            Day | Avg | Low | High | Margin
            {string.Join("\n", rows)}{accuracy}
            """;
    }

    private static string FormatAccuracy(IReadOnlyList<GetForecastEfficacyResponse> efficacy)
    {
        var model = efficacy
            .GroupBy(row => row.ModelName)
            .OrderByDescending(group => group.Sum(row => row.EvaluatedCount))
            .FirstOrDefault();

        var horizons = model
            ?.Where(row => row.MeanAbsolutePercentageError is not null)
            .GroupBy(row => row.HorizonDays)
            .Select(group => group.MaxBy(row => row.EvaluatedCount))
            .OfType<GetForecastEfficacyResponse>()
            .OrderBy(row => row.HorizonDays)
            .ToList();

        if (horizons is not { Count: > 0 })
        {
            return "";
        }

        var shown = horizons.Where(row => ForecastDays.Contains(row.HorizonDays)).ToList();
        if (shown.Count == 0)
        {
            shown = [.. horizons.Take(ForecastDays.Length)];
        }

        var errors = shown.Select(row =>
            $"{row.HorizonDays}d {Percent(row.MeanAbsolutePercentageError ?? 0)}"
        );
        var bias = shown[0].MeanBias switch
        {
            > 0 => "; forecasts ran high",
            < 0 => "; forecasts ran low",
            _ => "",
        };

        return $"\nError (MAPE, last {SymbolAnalyst.ForecastEfficacyWindow.Days}d, n={shown[0].EvaluatedCount}): "
            + $"{string.Join(", ", errors)}{bias}.";
    }

    private static string ComposePositions(SymbolAnalystSnapshot snapshot)
    {
        if (snapshot.Positions.Count == 0)
        {
            return """
                ## Your Positions
                No positions recorded.
                """;
        }

        var rows = snapshot.Positions.Select(position =>
            string.Join(
                " | ",
                position.CreatedAt.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                position.Side,
                position.Status,
                Compact(position.Quantity),
                Compact(position.Cost),
                Truncate(position.Notes ?? "", MaxNoteLength)
            )
        );

        return $"""
            ## Your Positions (most recent first; price per unit; Pending = planned, not filled)
            Created | Side | Status | Qty | Price | Notes
            {string.Join("\n", rows)}
            """;
    }

    private static string Truncate(string value, int maxLength)
    {
        var singleLine = value.ReplaceLineEndings(" ").Trim();

        return singleLine.Length <= maxLength ? singleLine : singleLine[..(maxLength - 1)] + "…";
    }

    private static string FormatChange(decimal ratio)
    {
        return (ratio > 0 ? "+" : "") + Percent(ratio);
    }

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    private static string FormatBool(bool value)
    {
        return value ? "yes" : "no";
    }

    private static string Compact(decimal value)
    {
        return AssistantNumberFormat.Compact(value);
    }

    private static string Percent(decimal ratio)
    {
        return AssistantNumberFormat.Percent(ratio);
    }

    private static string Number(decimal value)
    {
        return AssistantNumberFormat.Number(value);
    }
}
