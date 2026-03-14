namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketForecast;

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