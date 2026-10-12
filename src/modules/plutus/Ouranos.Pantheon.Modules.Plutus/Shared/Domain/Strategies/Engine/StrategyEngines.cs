using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;

public sealed class StrategyEngines(IEnumerable<IStrategyEngine> engines)
{
    private readonly Dictionary<StrategyMode, IStrategyEngine> _engines = Guard
        .Against.Null(engines)
        .ToDictionary(e => e.Mode);

    public IStrategyEngine For(StrategyConfiguration configuration)
    {
        Guard.Against.Null(configuration);

        return _engines.TryGetValue(configuration.Mode, out var engine)
            ? engine
            : throw new InvalidOperationException(
                $"No strategy engine is registered for {configuration.Mode}."
            );
    }
}
