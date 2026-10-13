using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;

/// <summary>
///     Everything specific to one <see cref="StrategyMode" />. Backtests, optimization and
///     recommendations are written once against this interface.
/// </summary>
public interface IStrategyEngine
{
    StrategyMode Mode { get; }

    /// <summary>Length of the in-sample slices an optimization candidate must do well in.</summary>
    int OptimizationSegmentDays { get; }

    /// <summary>Loads the history needed to simulate <paramref name="configuration" /> from start to end.</summary>
    Task<IStrategyMarketData> LoadAsync(
        Id<Market> marketId,
        DateTimeOffset start,
        DateTimeOffset end,
        StrategyConfiguration configuration,
        CancellationToken cancellationToken
    );

    SimulationResult Simulate(
        IStrategyMarketData data,
        StrategyConfiguration configuration,
        SimulationSettings settings
    );

    /// <summary>
    ///     The tunable parameters of <paramref name="seed" />, with its locked parameters pinned,
    ///     and how to rebuild a configuration from them.
    /// </summary>
    ParameterSpace CreateParameterSpace(StrategyConfiguration seed);

    /// <summary>What the strategy would buy right now with <paramref name="budget" /> and nothing held.</summary>
    Task<IReadOnlyList<StrategyRecommendation>> RecommendAsync(
        Id<Market> marketId,
        StrategyConfiguration configuration,
        decimal budget,
        CancellationToken cancellationToken
    );
}
