using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

/// <summary>
/// Shaping shared by the symbol analyst's prompt and tools.
/// </summary>
internal static class SymbolAnalystData
{
    public static SignalData Signal(
        SignalResponse signal,
        IReadOnlyList<decimal>? weeklyTrend = null
    )
    {
        return new SignalData(
            signal.Label,
            signal.Value,
            signal.Direction,
            signal.Strength,
            signal.Intents,
            weeklyTrend
        );
    }

    public static decimal Percent(decimal ratio)
    {
        return ratio * 100;
    }

    public static string Truncate(string value, int maxLength)
    {
        var singleLine = value.ReplaceLineEndings(" ").Trim();

        return singleLine.Length <= maxLength ? singleLine : singleLine[..(maxLength - 1)] + "…";
    }
}
