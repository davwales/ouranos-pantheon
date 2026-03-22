using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;

public sealed record SignalResponse(
    string Type,
    string Label,
    string Description,
    IReadOnlyList<InvestmentIntent> Intents,
    decimal Value,
    SignalDirection Direction,
    SignalStrength Strength
);
