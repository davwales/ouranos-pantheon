using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetConstructedForecasts;

public sealed record GetConstructedForecastsInput(
    IReadOnlyList<Symbol> Symbols,
    IReadOnlyDictionary<Id<Symbol>, List<ForecastPoint>> HistoricalData
) : IQuery<WrapperResponse<List<Forecast>>>;