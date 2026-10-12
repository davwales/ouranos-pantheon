using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class StrategyOptimizerTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = Start.AddDays(100);
    private static readonly DateTimeOffset InSampleEnd = Start.AddDays(80);

    private readonly StrategyOptimizer _optimizer = new(
        Options.Create(new OptimizationOptions()),
        Substitute.For<ILogger<StrategyOptimizer>>()
    );

    private static SimulationResult OpenOrdersDriven(
        StrategyConfiguration configuration,
        SimulationSettings settings
    )
    {
        return FakeStrategyEngine.Profitable(
            settings,
            ((FlipConfiguration)configuration).MaxOpenOrders
        );
    }

    private static Backtest CreateBacktest(
        FlipConfiguration? seed = null,
        int generations = 4,
        int populationSize = 8
    )
    {
        var marketId = new Id<Market>(Guid.NewGuid().ToString());

        var strategy = Strategy.Create(
            marketId,
            "Test",
            null,
            seed ?? new FlipConfiguration(MaxOpenOrders: 1)
        );

        return Backtest.Create(
            strategy.Id,
            marketId,
            Start,
            End,
            1000m,
            new BacktestRunSettings(
                0.2m,
                0.07m,
                new OptimizationSettings(
                    generations,
                    populationSize,
                    OutSampleRatio: 0.2,
                    MinTradesPerMonth: 0
                )
            ),
            strategy
        );
    }

    [Fact]
    public async Task OptimizeAsync_WhenFitnessGrowsWithParameter_ShouldMoveTowardBetterValue()
    {
        // Arrange
        var engine = new FakeStrategyEngine { OnSimulate = OpenOrdersDriven };
        var backtest = CreateBacktest();

        // Act
        var outcome = await _optimizer.OptimizeAsync(
            engine,
            engine.Data,
            backtest,
            new RecordingRunMonitor()
        );

        // Assert
        var optimized = outcome.Results.OptimizedConfiguration.ShouldBeOfType<FlipConfiguration>();
        optimized.MaxOpenOrders.ShouldBeGreaterThan(1);
        optimized.MaxOpenOrders.ShouldBeLessThanOrEqualTo(FakeStrategyEngine.MaxOpenOrdersLimit);
        optimized.CheckInHours.ShouldBe(FlipConfiguration.DefaultCheckInHours);
        outcome.Results.TotalTrades.ShouldBe(optimized.MaxOpenOrders);
        outcome.Positions.Count.ShouldBe(optimized.MaxOpenOrders);
    }

    [Fact]
    public async Task OptimizeAsync_WhenOptimized_ShouldCarryOutOfSampleResultsAndValidation()
    {
        // Arrange
        var engine = new FakeStrategyEngine { OnSimulate = OpenOrdersDriven };
        var backtest = CreateBacktest();

        // Act
        var outcome = await _optimizer.OptimizeAsync(
            engine,
            engine.Data,
            backtest,
            new RecordingRunMonitor()
        );

        // Assert
        outcome.Results.IsValidated.ShouldBeTrue();
        outcome.Results.OutSampleResults.ShouldNotBeNull();
        outcome.Results.OutSampleResults.TotalReturn.ShouldBeGreaterThan(0m);
        outcome.Positions.ShouldAllBe(p => p.EntryTime == Start && p.ExitTime == InSampleEnd);
        engine.Simulations.ShouldContain(s => s.Start == Start && s.End == InSampleEnd);
        engine.Simulations.ShouldContain(s => s.Start == InSampleEnd && s.End == End);
        engine.Simulations.ShouldAllBe(s =>
            s.Budget == 1000m && s.ParticipationRate == 0.2m && s.SlippageMultiplier == 0.07m
        );
    }

    [Fact]
    public async Task OptimizeAsync_WhenEveryConfigurationLoses_ShouldNotValidate()
    {
        // Arrange
        var engine = new FakeStrategyEngine
        {
            OnSimulate = (_, settings) => FakeStrategyEngine.Profitable(settings, 2, -10m),
        };

        var backtest = CreateBacktest(generations: 2, populationSize: 4);

        // Act
        var outcome = await _optimizer.OptimizeAsync(
            engine,
            engine.Data,
            backtest,
            new RecordingRunMonitor()
        );

        // Assert
        outcome.Results.IsValidated.ShouldBeFalse();
        outcome.Results.OutSampleResults.ShouldNotBeNull();
        outcome.Results.TotalReturn.ShouldBeLessThan(0m);
    }

    [Fact]
    public async Task OptimizeAsync_ShouldScoreCandidatesOnEngineSegments()
    {
        // Arrange
        var engine = new FakeStrategyEngine
        {
            OptimizationSegmentDays = 20,
            OnSimulate = OpenOrdersDriven,
        };

        var backtest = CreateBacktest(generations: 1, populationSize: 2);

        // Act
        await _optimizer.OptimizeAsync(engine, engine.Data, backtest, new RecordingRunMonitor());

        // Assert
        var windows = engine.Simulations.Select(s => (s.Start, s.End)).Distinct().ToList();
        windows.ShouldBe(
            [
                (Start, Start.AddDays(20)),
                (Start.AddDays(20), Start.AddDays(40)),
                (Start.AddDays(40), Start.AddDays(60)),
                (Start.AddDays(60), InSampleEnd),
                (Start, InSampleEnd),
                (InSampleEnd, End),
            ],
            ignoreOrder: true
        );
    }

    [Fact]
    public async Task OptimizeAsync_WhenNoCandidateIsViable_ShouldFallBackToSeedConfiguration()
    {
        // Arrange
        var seed = new FlipConfiguration(MaxOpenOrders: 3, MinMargin: 0.04m);

        var engine = new FakeStrategyEngine
        {
            OptimizationSegmentDays = 10,
            OnSimulate = (configuration, settings) =>
                settings.TotalDays <= 10
                    ? throw new InvalidOperationException("Segment failed.")
                    : OpenOrdersDriven(configuration, settings),
        };

        var backtest = CreateBacktest(seed, generations: 2, populationSize: 6);

        // Act
        var outcome = await _optimizer.OptimizeAsync(
            engine,
            engine.Data,
            backtest,
            new RecordingRunMonitor()
        );

        // Assert
        outcome.Results.OptimizedConfiguration.ShouldBe(seed);
        outcome.Results.TotalTrades.ShouldBe(3);
    }

    [Fact]
    public async Task OptimizeAsync_WhenEverySimulationThrows_ShouldThrow()
    {
        // Arrange
        var engine = new FakeStrategyEngine
        {
            OnSimulate = (_, _) => throw new InvalidOperationException("Simulation broke."),
        };

        var backtest = CreateBacktest(generations: 1, populationSize: 2);

        // Act
        var optimize = () =>
            _optimizer.OptimizeAsync(engine, engine.Data, backtest, new RecordingRunMonitor());

        // Assert
        var exception = await optimize.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Simulation broke.");
    }

    [Fact]
    public async Task OptimizeAsync_ShouldReportGenerationAndFinalRunProgress()
    {
        // Arrange
        var engine = new FakeStrategyEngine { OnSimulate = OpenOrdersDriven };
        var backtest = CreateBacktest(generations: 2, populationSize: 4);
        var monitor = new RecordingRunMonitor();

        // Act
        await _optimizer.OptimizeAsync(engine, engine.Data, backtest, monitor);

        // Assert
        monitor.Reports.ShouldContain(r => r.Message == "Optimizing: generation 1/2...");
        monitor.Reports.ShouldContain(r =>
            r.Percent == 90 && r.Message == "Optimizing: generation 2/2..."
        );
        monitor.Reports.Last().Percent.ShouldBe(92);
    }

    [Fact]
    public async Task OptimizeAsync_WhenMonitorCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var engine = new FakeStrategyEngine { OnSimulate = OpenOrdersDriven };
        var backtest = CreateBacktest();

        // Act
        var optimize = () =>
            _optimizer.OptimizeAsync(
                engine,
                engine.Data,
                backtest,
                new RecordingRunMonitor(new CancellationToken(true))
            );

        // Assert
        await optimize.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task OptimizeAsync_WhenBacktestIsNotAnOptimization_ShouldThrowArgumentNullException()
    {
        // Arrange
        var engine = new FakeStrategyEngine();
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var strategy = Strategy.Create(marketId, "Test", null, new FlipConfiguration());

        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            Start,
            End,
            1000m,
            new BacktestRunSettings(),
            strategy
        );

        // Act
        var optimize = () =>
            _optimizer.OptimizeAsync(engine, engine.Data, backtest, new RecordingRunMonitor());

        // Assert
        await optimize.ShouldThrowAsync<ArgumentNullException>();
    }
}
