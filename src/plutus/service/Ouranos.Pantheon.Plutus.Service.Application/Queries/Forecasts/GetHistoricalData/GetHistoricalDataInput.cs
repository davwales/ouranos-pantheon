using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetHistoricalData;

public sealed record GetHistoricalDataInput(
    IReadOnlyList<Id<Symbol>> SymbolIds
) : IQuery<WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>>;