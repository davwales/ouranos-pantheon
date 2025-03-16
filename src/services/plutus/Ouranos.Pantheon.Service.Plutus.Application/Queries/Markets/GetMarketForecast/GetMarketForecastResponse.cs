using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Markets.GetMarketForecast;

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