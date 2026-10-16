using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

public sealed record SignalData(
    string Name,
    decimal Value,
    SignalDirection Direction,
    SignalStrength Strength,
    IReadOnlyList<InvestmentIntent> Intents,
    IReadOnlyList<decimal>? WeeklyTrend = null
);
