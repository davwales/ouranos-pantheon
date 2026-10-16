using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst;

/// <summary>
/// Shaping shared by more than one market analyst tool.
/// </summary>
internal static class MarketAnalystData
{
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
}
