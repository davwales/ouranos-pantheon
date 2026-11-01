using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;

/// <summary>Narrows <see cref="IStrategyEngine" /> to one configuration and data type.</summary>
public abstract class StrategyEngine<TConfiguration, TData> : IStrategyEngine
    where TConfiguration : StrategyConfiguration
    where TData : IStrategyMarketData
{
    public abstract StrategyMode Mode { get; }

    public abstract int OptimizationSegmentDays { get; }

    async Task<IStrategyMarketData> IStrategyEngine.LoadAsync(
        Id<Market> marketId,
        DateTimeOffset start,
        DateTimeOffset end,
        StrategyConfiguration configuration,
        CancellationToken cancellationToken
    )
    {
        return await LoadAsync(marketId, start, end, Cast(configuration), cancellationToken);
    }

    SimulationResult IStrategyEngine.Simulate(
        IStrategyMarketData data,
        StrategyConfiguration configuration,
        SimulationSettings settings
    )
    {
        if (data is not TData typed)
        {
            throw new ArgumentException(
                $"{Mode} engine cannot simulate {data.GetType().Name}.",
                nameof(data)
            );
        }

        return Simulate(typed, Cast(configuration), settings);
    }

    ParameterSpace IStrategyEngine.CreateParameterSpace(StrategyConfiguration seed)
    {
        return CreateParameterSpace(Cast(seed)).Lock(seed.LockedParameters);
    }

    Task<IReadOnlyList<StrategyRecommendation>> IStrategyEngine.RecommendAsync(
        Id<Market> marketId,
        StrategyConfiguration configuration,
        decimal budget,
        decimal participationRate,
        CancellationToken cancellationToken
    )
    {
        return RecommendAsync(
            marketId,
            Cast(configuration),
            budget,
            participationRate,
            cancellationToken
        );
    }

    protected abstract Task<TData> LoadAsync(
        Id<Market> marketId,
        DateTimeOffset start,
        DateTimeOffset end,
        TConfiguration configuration,
        CancellationToken cancellationToken
    );

    protected abstract SimulationResult Simulate(
        TData data,
        TConfiguration configuration,
        SimulationSettings settings
    );

    protected abstract ParameterSpace CreateParameterSpace(TConfiguration seed);

    protected abstract Task<IReadOnlyList<StrategyRecommendation>> RecommendAsync(
        Id<Market> marketId,
        TConfiguration configuration,
        decimal budget,
        decimal participationRate,
        CancellationToken cancellationToken
    );

    private TConfiguration Cast(StrategyConfiguration configuration)
    {
        return configuration as TConfiguration
            ?? throw new ArgumentException(
                $"{Mode} engine cannot run a {configuration.Mode} configuration.",
                nameof(configuration)
            );
    }
}
