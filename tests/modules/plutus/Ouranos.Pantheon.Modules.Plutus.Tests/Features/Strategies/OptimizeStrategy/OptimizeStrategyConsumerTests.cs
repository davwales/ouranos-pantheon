using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyConsumerTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = Start.AddDays(100);

    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory =
        DbContextExtensions.MockFactory<PlutusDbContext>();

    private OptimizeStrategyConsumer CreateConsumer(params IStrategyEngine[] engines)
    {
        var runner = new BacktestJobRunner(
            _dbContextFactory,
            Substitute.For<ILogger<BacktestJobRunner>>()
        )
        {
            MonitorInterval = TimeSpan.FromMilliseconds(20),
        };

        var optimizer = new StrategyOptimizer(
            Options.Create(new OptimizationOptions()),
            Substitute.For<ILogger<StrategyOptimizer>>()
        );

        return new OptimizeStrategyConsumer(
            Substitute.For<ILogger<OptimizeStrategyConsumer>>(),
            runner,
            new StrategyEngines(engines),
            optimizer
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCompleteWithOptimizedConfiguration()
    {
        // Arrange
        var backtest = await SeedBacktestAsync();

        var engine = new FakeStrategyEngine
        {
            OnSimulate = (configuration, settings) =>
                FakeStrategyEngine.Profitable(
                    settings,
                    ((FlipConfiguration)configuration).MaxOpenOrders
                ),
        };

        var consumer = CreateConsumer(engine);

        // Act
        await consumer.Handle(new OptimizeStrategyMessage(backtest.Id), CancellationToken.None);

        // Assert
        var load = engine.Loads.ShouldHaveSingleItem();
        load.Start.ShouldBe(Start);
        load.End.ShouldBe(End);

        var saved = await LoadAsync(backtest.Id);
        saved.Status.ShouldBe(BacktestStatus.Completed);
        saved.Kind.ShouldBe(BacktestKind.Optimization);
        saved.Results.ShouldNotBeNull();

        var optimized = saved.Results.OptimizedConfiguration.ShouldBeOfType<FlipConfiguration>();
        optimized.MaxOpenOrders.ShouldBeGreaterThan(1);
        saved.Results.OutSampleResults.ShouldNotBeNull();
        saved.Results.IsValidated.ShouldBeTrue();
        saved.Positions.Count.ShouldBe(optimized.MaxOpenOrders);
    }

    [Fact]
    public async Task Handle_WhenEverySimulationFails_ShouldFailBacktest()
    {
        // Arrange
        var backtest = await SeedBacktestAsync();

        var engine = new FakeStrategyEngine
        {
            OnSimulate = (_, _) => throw new InvalidOperationException("Simulation broke."),
        };

        var consumer = CreateConsumer(engine);

        // Act
        await consumer.Handle(new OptimizeStrategyMessage(backtest.Id), CancellationToken.None);

        // Assert
        var saved = await LoadAsync(backtest.Id);
        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldBe("Simulation broke.");
    }

    [Fact]
    public async Task Handle_WhenBacktestMissing_ShouldNotLoadData()
    {
        // Arrange
        var engine = new FakeStrategyEngine();
        var consumer = CreateConsumer(engine);

        // Act
        await consumer.Handle(
            new OptimizeStrategyMessage(new Id<Backtest>(Guid.NewGuid().ToString())),
            CancellationToken.None
        );

        // Assert
        engine.Loads.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var consumer = CreateConsumer(new FakeStrategyEngine());

        // Act
        var handle = () =>
            consumer.Handle(
                new OptimizeStrategyMessage(new Id<Backtest>(Guid.NewGuid().ToString())),
                new CancellationToken(true)
            );

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<Backtest> SeedBacktestAsync()
    {
        var marketId = new Id<Market>(Guid.NewGuid().ToString());

        var strategy = Strategy.Create(
            marketId,
            "Test",
            null,
            new FlipConfiguration(MaxOpenOrders: 1)
        );

        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            Start,
            End,
            1000m,
            new BacktestRunSettings(
                Optimization: new OptimizationSettings(3, 8, MinTradesPerMonth: 0)
            ),
            strategy
        );

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.SeedData(strategy);
        await dbContext.SeedData(backtest);
        return backtest;
    }

    private async Task<Backtest> LoadAsync(Id<Backtest> backtestId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Backtests.AsNoTracking().SingleAsync(b => b.Id == backtestId);
    }
}
