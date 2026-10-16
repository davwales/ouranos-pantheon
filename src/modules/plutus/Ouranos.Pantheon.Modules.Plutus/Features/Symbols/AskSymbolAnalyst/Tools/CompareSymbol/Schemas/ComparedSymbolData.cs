using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.CompareSymbol.Schemas;

public sealed record ComparedSymbolData(
    string Name,
    IReadOnlyList<PriceSummaryData> PriceSummaries,
    SignalSummary SignalScore,
    IReadOnlyList<SignalData> StrongestSignals
);
