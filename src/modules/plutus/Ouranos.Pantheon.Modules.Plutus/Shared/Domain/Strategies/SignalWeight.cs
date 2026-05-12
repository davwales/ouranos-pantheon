using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record SignalWeight(SignalType Type, decimal Weight)
{
    private SignalWeight()
        : this(default, 0) { }
}
