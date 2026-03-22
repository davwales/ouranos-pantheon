using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;

public sealed record GetSymbolSignalsInput(
    Id<Symbol> SymbolId,
    InvestmentIntent? Intent = null
);
