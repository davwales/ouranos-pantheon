using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;

/// <summary>
/// Shaping shared by the assistant tools and the assistant prompts.
/// </summary>
internal static class AssistantToolData
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

    public static ForecastOutlookData ForecastOutlook(GetMarketForecastResponse forecast)
    {
        return new ForecastOutlookData(
            forecast.Latest.AveragePrice,
            forecast.DayOne.AveragePrice,
            forecast.DayOne.Gain,
            forecast.DaySeven.AveragePrice,
            forecast.DaySeven.Gain
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
