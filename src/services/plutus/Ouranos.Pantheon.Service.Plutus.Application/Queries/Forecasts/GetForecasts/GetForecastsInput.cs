using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetForecasts;

public sealed record GetForecastsInput(
    IReadOnlyList<Symbol> Symbols,
    IReadOnlyDictionary<Id<Symbol>, List<ForecastPoint>> HistoricalData
) : IQuery<WrapperResponse<List<Forecast>>>;