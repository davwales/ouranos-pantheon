using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetHistoricalData;

public sealed record GetHistoricalDataInput(
    IReadOnlyList<Id<Symbol>> SymbolIds
) : IQuery<WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>>;