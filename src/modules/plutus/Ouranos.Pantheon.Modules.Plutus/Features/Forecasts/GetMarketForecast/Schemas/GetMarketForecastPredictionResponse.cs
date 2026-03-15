namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;

public sealed record GetMarketForecastPredictionResponse(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume,
    decimal Margin,
    decimal Gain,
    decimal AveragePriceDelta,
    decimal MinPriceDelta,
    decimal MaxPriceDelta,
    decimal VolumeDelta,
    decimal GainDelta
);
