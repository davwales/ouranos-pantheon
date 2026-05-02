namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

public sealed class SignalWeightedExecutor : IStrategyExecutor
{
    public StrategyType SupportedType => StrategyType.SignalWeighted;

    public decimal? Score(StrategyScoreContext context, StrategyConfiguration configuration)
    {
        if (context.Signals.Count == 0)
        {
            return null;
        }

        if (configuration.SignalWeights is null || configuration.SignalWeights.Count == 0)
        {
            var values = context.Signals.Where(s => s.Value != 0).Select(s => s.Value).ToList();
            return values.Count > 0 ? values.Average() : null;
        }

        var weightMap = configuration.SignalWeights
            .Where(w => w.Weight != 0)
            .ToDictionary(w => w.Type, w => w.Weight);

        if (weightMap.Count == 0)
        {
            return null;
        }

        var weightedSum = 0m;
        var totalWeight = 0m;

        foreach (var signal in context.Signals)
        {
            if (weightMap.TryGetValue(signal.Type, out var weight))
            {
                weightedSum += signal.Value * weight;
                totalWeight += weight;
            }
        }

        return totalWeight > 0 ? weightedSum / totalWeight : null;
    }
}