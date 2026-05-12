using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;

public sealed record GetSymbolSignalsResponse(
    Id<Symbol> SymbolId,
    string SymbolName,
    IReadOnlyList<SignalResponse> Signals,
    SignalSummary Summary
);
