using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignals.Schemas;

public sealed record SignalsData(SignalSummary Score, IReadOnlyList<SignalData> Signals);
