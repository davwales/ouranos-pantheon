namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

public sealed class CompositeExecutor : IStrategyExecutor
{
    private readonly Dictionary<StrategyType, IStrategyExecutor> _executors;

    public StrategyType SupportedType => StrategyType.Composite;

    public CompositeExecutor(IEnumerable<IStrategyExecutor> executors)
    {
        _executors = executors
            .Where(e => e.SupportedType != StrategyType.Composite)
            .ToDictionary(e => e.SupportedType);
    }

    public decimal? Score(StrategyScoreContext context, StrategyConfiguration configuration)
    {
        if (configuration.Components is null || configuration.Components.Count == 0)
        {
            return null;
        }

        var weightedSum = 0m;
        var totalWeight = 0m;

        foreach (var component in configuration.Components)
        {
            if (!_executors.TryGetValue(component.Type, out var executor))
            {
                continue;
            }

            var subScore = executor.Score(context, configuration);
            if (subScore is null)
            {
                continue;
            }

            weightedSum += subScore.Value * component.Weight;
            totalWeight += component.Weight;
        }

        if (totalWeight == 0)
        {
            return null;
        }

        return Math.Clamp(weightedSum / totalWeight, -1m, 1m);
    }
}