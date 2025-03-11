using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Symbols.GetSymbolTrades;

public sealed record GetSymbolTradesInput(
    Id<Symbol> SymbolId,
    int NumBuckets = 100,
    double? Seconds = null
) : IQuery<GetSymbolTradesResponse>;