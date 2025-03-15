using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetForecasts;

public sealed record GetForecastsResponse(
    Id<Forecast> Id,
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string SymbolName,
    string? SymbolSubcode,
    ForecastPoint Latest,
    GetForecastsPredictionResponse DayOne,
    GetForecastsPredictionResponse DayTwo,
    GetForecastsPredictionResponse DayThree,
    GetForecastsPredictionResponse DayFour,
    GetForecastsPredictionResponse DayFive,
    GetForecastsPredictionResponse DaySix,
    GetForecastsPredictionResponse DaySeven
);