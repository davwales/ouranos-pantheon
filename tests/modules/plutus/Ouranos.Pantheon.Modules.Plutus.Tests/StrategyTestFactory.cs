using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

namespace Ouranos.Pantheon.Modules.Plutus.Tests;

internal static class StrategyTestFactory
{
    public static List<InputWeight> DefaultWeights()
    {
        return [new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m)];
    }

    public static SwingConfiguration Swing()
    {
        return new SwingConfiguration(DefaultWeights());
    }
}
