using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetSymbolsToForecast;

public sealed record GetSymbolsToForecastInput : IQuery<WrapperResponse<List<Symbol>>>;