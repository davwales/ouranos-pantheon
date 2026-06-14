using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;

public sealed record GetSymbolSignalHistoryResponse(
    Id<Symbol> SymbolId,
    string SymbolName,
    IReadOnlyList<SignalHistoryResponse> Signals,
    SignalSummary Summary
);
