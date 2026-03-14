using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Symbols.GetSymbolTrades;

public sealed record GetSymbolTradesInput(
    Id<Symbol> SymbolId,
    int NumBuckets = 100,
    double? Seconds = null
) : IQuery<GetSymbolTradesResponse>;