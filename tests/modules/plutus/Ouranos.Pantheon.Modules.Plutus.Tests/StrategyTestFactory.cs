using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Tests;

internal static class StrategyTestFactory
{
    public static List<InputWeight> DefaultWeights()
    {
        return [new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m)];
    }

    public static IReadOnlyList<IInputScorer> DefaultScorers()
    {
        return
        [
            new TaxAdjustedRoiInputScorer(),
            new VolumeAnomalyInputScorer(),
            new TrendMomentumInputScorer(),
            new BollingerBandsInputScorer(),
            new RsiInputScorer(),
            new MovingAverageCrossoverInputScorer(),
            new PriceVelocityInputScorer(),
        ];
    }
}
