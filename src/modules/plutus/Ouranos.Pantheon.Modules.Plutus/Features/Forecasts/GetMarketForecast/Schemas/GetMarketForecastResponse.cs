using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;

public sealed record GetMarketForecastResponse(
    Id<Forecast> Id,
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string SymbolName,
    string? SymbolSubcode,
    ForecastPoint Latest,
    GetMarketForecastPredictionResponse DayOne,
    GetMarketForecastPredictionResponse DayTwo,
    GetMarketForecastPredictionResponse DayThree,
    GetMarketForecastPredictionResponse DayFour,
    GetMarketForecastPredictionResponse DayFive,
    GetMarketForecastPredictionResponse DaySix,
    GetMarketForecastPredictionResponse DaySeven
);
