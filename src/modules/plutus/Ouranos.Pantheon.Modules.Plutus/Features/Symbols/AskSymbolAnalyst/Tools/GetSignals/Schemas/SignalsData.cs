using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetSignals.Schemas;

public sealed record SignalsData(SignalSummary Score, IReadOnlyList<SignalData> Signals);
