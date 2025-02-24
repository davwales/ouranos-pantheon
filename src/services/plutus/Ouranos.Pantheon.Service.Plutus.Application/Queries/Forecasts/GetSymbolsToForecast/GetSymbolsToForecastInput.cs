using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetSymbolsToForecast;

public sealed record GetSymbolsToForecastInput : IQuery<WrapperResponse<List<Id<Symbol>>>>;