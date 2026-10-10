using System.Globalization;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

internal static class SymbolAnalystPrompt
{
    public const string Instructions = """
        You are a trading analyst helping a user decide what to do with the single symbol below.
        Ground every claim in the data provided and quote the relevant numbers. Account for the
        market tax whenever you discuss margins, profit or sell prices. Investment intents: Buy
        and Sell are directional calls, Flip is a short-term trade capturing the spread between
        the low and high price, and Merch is a speculative hold for a larger price move. Signal
        values range from -1 (bearish) to 1 (bullish). Weigh forecasts by their measured accuracy.
        When the data is missing, stale or insufficient to answer, say so instead of guessing, and
        never invent prices or trades. Keep answers concise and give concrete price levels when
        asked. Note the uncertainty in any trade suggestion and that it is not financial advice.
        All times are UTC.
        """;

    private const string DateTimeFormat = "yyyy-MM-dd HH:mm";

    public static string Compose(SymbolAnalystSnapshot snapshot)
    {
        string[] sections =
        [
            Instructions,
            ComposeSymbol(snapshot),
            ComposeTaxes(snapshot),
            ComposeLatestTrade(snapshot),
            ComposeWindows(snapshot),
            ComposeChart(snapshot),
            ComposeSignals(snapshot),
            ComposeForecast(snapshot),
            ComposeForecastAccuracy(snapshot),
            ComposePositions(snapshot),
        ];

        return string.Join("\n\n", sections.Where(section => section.Length > 0));
    }

    private static string ComposeSymbol(SymbolAnalystSnapshot snapshot)
    {
        var symbol = snapshot.Symbol;
        var fields = symbol.AdditionalFields;
        var lines = new List<string> { $"- Code: {symbol.Code}" };

        if (!string.IsNullOrWhiteSpace(symbol.Subcode))
        {
            lines.Add($"- Subcode: {symbol.Subcode}");
        }

        lines.Add($"- Market: {snapshot.Market.Name}");

        if (!string.IsNullOrWhiteSpace(snapshot.Market.Description))
        {
            lines.Add($"- Market description: {snapshot.Market.Description}");
        }

        if (fields.Limit is { } limit)
        {
            lines.Add($"- Buy limit: {FormatNumber(limit)}");
        }

        if (FormatAlchemy(fields) is { } alchemy)
        {
            lines.Add(alchemy);
        }

        if (!string.IsNullOrWhiteSpace(fields.Exchange))
        {
            lines.Add($"- Exchange: {fields.Exchange}");
        }

        lines.Add($"- Data as of: {FormatDate(snapshot.GeneratedAt)}");

        return $"""
            # Symbol: {symbol.Name}

            {string.Join("\n", lines)}
            """;
    }

    // Small models mistake the alchemy values for the market's low and high prices unless they
    // are explicitly labelled as fixed NPC prices.
    private static string? FormatAlchemy(AdditionalFields fields)
    {
        var values = new List<string>();

        if (fields.HighAlch is { } high)
        {
            values.Add($"high {FormatNumber(high)}");
        }

        if (fields.LowAlch is { } low)
        {
            values.Add($"low {FormatNumber(low)}");
        }

        return values.Count == 0
            ? null
            : $"- Alchemy values (fixed NPC sale prices, not market prices): {string.Join(", ", values)}";
    }

    private static string ComposeTaxes(SymbolAnalystSnapshot snapshot)
    {
        var flat = snapshot.Market.Taxes.Flat;
        var description = flat is null
            ? "No tax is charged on sales."
            : $"Sales are taxed at {FormatPercent(flat.Rate)} of the sell price per item "
                + $"(minimum {FormatNumber(flat.Minimum)}, capped at {FormatNumber(flat.Maximum)}).";

        return $"""
            ## Market Taxes

            {description}
            """;
    }

    private static string ComposeLatestTrade(SymbolAnalystSnapshot snapshot)
    {
        var trade = snapshot.LatestTrade;
        var description = trade is null
            ? "No trades recorded in the last year."
            : $"Price {FormatNumber(trade.Price)}, volume {FormatNumber(trade.Volume)}, "
                + $"at {FormatDate(trade.Timestamp)}.";

        return $"""
            ## Latest Trade

            {description}
            """;
    }

    private static string ComposeWindows(SymbolAnalystSnapshot snapshot)
    {
        var rows = snapshot.Windows.Select(window =>
            window.Trades is { TotalVolume: > 0 } trades
                ? FormatWindow(window.TimeFrame.ToString(), trades)
                : $"| {window.TimeFrame} | no trades | | | | | | | |"
        );

        return $"""
            ## Price Summary

            Margin is the high price minus the low price minus tax; ROI is margin over the low price.

            | Window | Avg | Low | High | Volume | Trades | Tax | Margin | ROI |
            |---|---|---|---|---|---|---|---|---|
            {string.Join("\n", rows)}
            """;
    }

    private static string FormatWindow(string label, GetMarketTradesResponse trades)
    {
        var roi = trades.MinPrice > 0 ? FormatPercent(trades.Roi) : "-";

        return $"| {label} | {FormatNumber(trades.AveragePrice)} | {FormatNumber(trades.MinPrice)} "
            + $"| {FormatNumber(trades.MaxPrice)} | {FormatNumber(trades.TotalVolume)} "
            + $"| {trades.NumTransactions} | {FormatNumber(trades.Tax)} "
            + $"| {FormatNumber(trades.Margin)} | {roi} |";
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

        var rows = trades.Trades.Select(bucket =>
            $"| {FormatDate(bucket.Date)} | {FormatNumber(bucket.OpenPrice)} "
            + $"| {FormatNumber(bucket.ClosePrice)} | {FormatNumber(bucket.Price)} "
            + $"| {FormatNumber(bucket.MinPrice)} | {FormatNumber(bucket.MaxPrice)} "
            + $"| {FormatNumber(bucket.Volume)} |"
        );

        return $"""
            {heading}

            Average {FormatNumber(trades.AveragePrice)}, low {FormatNumber(
                trades.MinPrice
            )}, high {FormatNumber(trades.MaxPrice)}, volume {FormatNumber(
                trades.Volume
            )}, {trades.NumTransactions} trades.

            | Start | Open | Close | Avg | Low | High | Volume |
            |---|---|---|---|---|---|---|
            {string.Join("\n", rows)}
            """;
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
        var rows = signals.Signals.Select(signal =>
            $"| {signal.Label} | {FormatNumber(signal.Value)} | {signal.Direction} "
            + $"| {signal.Strength} | {string.Join(", ", signal.Intents)} |"
        );
        var legend = signals.Signals.Select(signal => $"- {signal.Label}: {signal.Description}");

        var current = $"""
            ## Signals

            Aggregated score {FormatNumber(
                summary.AggregatedScore
            )}; {summary.BullishCount} bullish, {summary.BearishCount} bearish, {summary.NeutralCount} neutral. Flip favourable: {FormatBool(
                summary.IsFlipFavourable
            )}. Merch favourable: {FormatBool(summary.IsMerchFavourable)}.

            | Signal | Value | Direction | Strength | Intents |
            |---|---|---|---|---|
            {string.Join("\n", rows)}

            ### Signal Definitions

            {string.Join("\n", legend)}
            """;

        var trends = snapshot
            .SignalHistory.Signals.Where(signal => signal.History.Count > 0)
            .Select(FormatTrend)
            .ToList();

        if (trends.Count == 0)
        {
            return current;
        }

        return $"""
            {current}

            ### Daily Trend (last 7 days, oldest first)

            {string.Join("\n", trends)}
            """;
    }

    private static string FormatTrend(SignalHistoryResponse signal)
    {
        var daily = signal
            .History.GroupBy(point => point.ComputedAt.UtcDateTime.Date)
            .OrderBy(day => day.Key)
            .Select(day => FormatNumber(day.OrderBy(point => point.ComputedAt).Last().Value));

        return $"- {signal.Label}: {string.Join(" → ", daily)}";
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
        var rows = days.Select(
            (day, i) =>
                $"| {i + 1} | {FormatNumber(day.AveragePrice)} | {FormatNumber(day.MinPrice)} "
                + $"| {FormatNumber(day.MaxPrice)} | {FormatNumber(day.Volume)} "
                + $"| {FormatNumber(day.Margin)} |"
        );
        var latest = forecast.Latest;

        return $"""
            ## Forecast

            Baseline (latest actual day): average {FormatNumber(
                latest.AveragePrice
            )}, low {FormatNumber(latest.MinPrice)}, high {FormatNumber(
                latest.MaxPrice
            )}, volume {FormatNumber(latest.Volume)}.

            | Day | Avg | Low | High | Volume | Margin |
            |---|---|---|---|---|---|
            {string.Join("\n", rows)}
            """;
    }

    private static string ComposeForecastAccuracy(SymbolAnalystSnapshot snapshot)
    {
        if (snapshot.ForecastEfficacy.Count == 0)
        {
            return string.Empty;
        }

        var rows = snapshot.ForecastEfficacy.Select(efficacy =>
            $"| {efficacy.HorizonDays} | {efficacy.EvaluatedCount} "
            + $"| {FormatOptionalPercent(efficacy.MeanAbsolutePercentageError)} "
            + $"| {FormatOptionalNumber(efficacy.MeanAbsoluteError)} "
            + $"| {FormatOptionalNumber(efficacy.MeanBias)} |"
        );

        return $"""
            ## Forecast Accuracy (last 30 days)

            Bias is the mean of predicted minus actual average price; positive means forecasts ran high.

            | Horizon (days) | Evaluated | MAPE | MAE | Bias |
            |---|---|---|---|---|
            {string.Join("\n", rows)}
            """;
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
            $"| {FormatDate(position.CreatedAt)} | {position.Side} | {position.Status} "
            + $"| {FormatNumber(position.Quantity)} | {FormatNumber(position.Cost)} "
            + $"| {position.Notes ?? string.Empty} |"
        );

        return $"""
            ## Your Positions

            Most recent first. Price is per unit. Pending positions are planned but not yet filled.

            | Created | Side | Status | Quantity | Price | Notes |
            |---|---|---|---|---|---|
            {string.Join("\n", rows)}
            """;
    }

    private static string FormatNumber(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatOptionalNumber(decimal? value)
    {
        return value is { } number ? FormatNumber(number) : "-";
    }

    private static string FormatPercent(decimal ratio)
    {
        return (ratio * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }

    private static string FormatOptionalPercent(decimal? ratio)
    {
        return ratio is { } value ? FormatPercent(value) : "-";
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
    }

    private static string FormatBool(bool value)
    {
        return value ? "yes" : "no";
    }
}
