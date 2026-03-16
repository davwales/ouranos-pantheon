using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;

public sealed record GetSymbolTradesInput(
    Id<Symbol> SymbolId,
    int NumBuckets = 100,
    double? Seconds = null
) : IQuery<GetSymbolTradesResponse>;
