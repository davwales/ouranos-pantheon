using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;

public sealed record GetSymbolTradesInput(
    Id<Symbol> SymbolId,
    TimeFrame TimeFrame = TimeFrame.OneHour,
    int NumBuckets = 100
);
