using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;

public sealed record SignalHistoryResponse(
    string Type,
    string Label,
    string Description,
    IReadOnlyList<InvestmentIntent> Intents,
    decimal CurrentValue,
    SignalDirection Direction,
    SignalStrength Strength,
    IReadOnlyList<SignalHistoryPoint> History
);
