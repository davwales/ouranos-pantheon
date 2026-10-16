using System.Globalization;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

internal static class SymbolAnalystPrompt
{
    public const string Instructions = """
        You are a trading analyst for the single symbol below. Use only the data you are given
        and quote the numbers you rely on; never invent prices or trades, and say so when data is
        missing or stale. Margin and ROI are after tax; apply the tax to any sell price you
        suggest. Intents: Buy and Sell are directional, Flip trades the low-high spread, Merch
        holds for a larger move. Be concise, give concrete price levels, and note the uncertainty
        and that this is not financial advice. K, M, B, and T mean thousand, million, billion,
        and trillion.
        Times are UTC. The data below is only a headline: the symbol's facts and today's prices so
        far. It leaves out the latest trade, longer price history, margins, signals, forecasts and
        positions. When a question depends on detail that is not shown, look it up if you can and
        otherwise say it is not shown; never infer it from the headline.
        """;

    internal const int MaxDescriptionLength = 80;

    public static string Compose(SymbolAnalystSnapshot snapshot)
    {
        string[] sections = [Instructions, ComposeSymbol(snapshot), ComposeToday(snapshot.Today)];

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
            : $"{snapshot.Market.Name} ({SymbolAnalystData.Truncate(snapshot.Market.Description, MaxDescriptionLength)})";

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

        if (!snapshot.Market.IsForecastingEnabled)
        {
            lines.Add("- Forecasting is disabled for this market");
        }

        lines.Add($"- The user is viewing the {snapshot.TimeFrame} chart");
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

    private static string ComposeToday(GetDailySymbolSummaryResponse today)
    {
        var summary =
            today.Volume > 0
                ? $"Avg {Compact(today.AveragePrice)}, low {Compact(today.MinPrice)}, "
                    + $"high {Compact(today.MaxPrice)}, vol {Compact(today.Volume)}."
                : "No trades yet today.";

        return $"""
            ## Today So Far
            {summary}
            """;
    }

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    private static string Compact(decimal value)
    {
        return AssistantNumberFormat.Compact(value);
    }

    private static string Percent(decimal ratio)
    {
        return AssistantNumberFormat.Percent(ratio);
    }
}
