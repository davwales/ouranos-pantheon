using System.Collections.Concurrent;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests;

internal sealed class FakeStrategyEngine(StrategyMode mode = StrategyMode.Flip) : IStrategyEngine
{
    public const int MaxOpenOrdersLimit = 10;

    private readonly ConcurrentQueue<SimulationSettings> _simulations = new();
    private readonly ConcurrentQueue<StrategyConfiguration> _simulatedConfigurations = new();

    public StrategyMode Mode { get; } = mode;

    public int OptimizationSegmentDays { get; init; } = 30;

    public FakeStrategyMarketData Data { get; } = new();

    public Func<CancellationToken, Task> OnLoad { get; init; } = _ => Task.CompletedTask;

    public Func<
        StrategyConfiguration,
        SimulationSettings,
        SimulationResult
    > OnSimulate { get; init; } = (_, settings) => Profitable(settings, 1);

    public IReadOnlyList<StrategyRecommendation> Recommendations { get; init; } = [];

    public List<(
        Id<Market> MarketId,
        DateTimeOffset Start,
        DateTimeOffset End,
        StrategyConfiguration Configuration
    )> Loads { get; } = [];

    public List<(
        Id<Market> MarketId,
        StrategyConfiguration Configuration,
        decimal Budget,
        decimal ParticipationRate
    )> RecommendCalls { get; } = [];

    public IReadOnlyCollection<SimulationSettings> Simulations => _simulations;

    public IReadOnlyCollection<StrategyConfiguration> SimulatedConfigurations =>
        _simulatedConfigurations;

    public async Task<IStrategyMarketData> LoadAsync(
        Id<Market> marketId,
        DateTimeOffset start,
        DateTimeOffset end,
        StrategyConfiguration configuration,
        CancellationToken cancellationToken
    )
    {
        Loads.Add((marketId, start, end, configuration));
        await OnLoad(cancellationToken);
        return Data;
    }

    public SimulationResult Simulate(
        IStrategyMarketData data,
        StrategyConfiguration configuration,
        SimulationSettings settings
    )
    {
        _simulations.Enqueue(settings);
        _simulatedConfigurations.Enqueue(configuration);
        settings.Progress?.Report(1.0);
        return OnSimulate(configuration, settings);
    }

    public ParameterSpace CreateParameterSpace(StrategyConfiguration seed)
    {
        if (seed is not FlipConfiguration flip)
        {
            return new ParameterSpace([], [], _ => seed);
        }

        return new ParameterSpace(
            [
                new Parameter("maxOpenOrders", 1, MaxOpenOrdersLimit, IsWhole: true),
                new Parameter("maxHoldHours", 12, 168, IsWhole: true),
            ],
            [flip.MaxOpenOrders, flip.MaxHoldHours],
            values => flip with { MaxOpenOrders = (int)values[0], MaxHoldHours = (int)values[1] }
        ).Lock(seed.LockedParameters);
    }

    public Task<IReadOnlyList<StrategyRecommendation>> RecommendAsync(
        Id<Market> marketId,
        StrategyConfiguration configuration,
        decimal budget,
        decimal participationRate,
        CancellationToken cancellationToken
    )
    {
        RecommendCalls.Add((marketId, configuration, budget, participationRate));
        return Task.FromResult(Recommendations);
    }

    public static SimulationResult Profitable(
        SimulationSettings settings,
        int symbols,
        decimal profitPerSymbol = 10m
    )
    {
        var positions = Enumerable
            .Range(0, symbols)
            .Select(i => new BacktestPosition(
                $"S{i}",
                $"Symbol {i}",
                100m,
                100m + profitPerSymbol,
                1m,
                profitPerSymbol,
                profitPerSymbol / 100m,
                settings.Start,
                settings.End
            ))
            .ToList();

        var finalBalance = settings.Budget + (symbols * profitPerSymbol);
        var equity = new EquityCurve(settings.Budget);
        equity.Record(settings.Budget);
        equity.Record(finalBalance);
        return new SimulationResult(positions, equity, finalBalance);
    }
}
