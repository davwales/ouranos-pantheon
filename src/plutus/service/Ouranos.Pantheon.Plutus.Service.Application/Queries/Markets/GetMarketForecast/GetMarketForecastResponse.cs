using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketForecast;

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