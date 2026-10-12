using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumerTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = Start.AddDays(60);

    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory =
        DbContextExtensions.MockFactory<PlutusDbContext>();

    private RunBacktestConsumer CreateConsumer(params IStrategyEngine[] engines)
    {
        var runner = new BacktestJobRunner(
            _dbContextFactory,
            Substitute.For<ILogger<BacktestJobRunner>>()
        )
        {
            MonitorInterval = TimeSpan.FromMilliseconds(20),
        };

        return new RunBacktestConsumer(
            Substitute.For<ILogger<RunBacktestConsumer>>(),
            runner,
            new StrategyEngines(engines)
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldSimulateWithRunSettingsAndComplete()
    {
        // Arrange
        var configuration = new FlipConfiguration(MaxOpenOrders: 3);
        var backtest = await SeedBacktestAsync(configuration, new BacktestRunSettings(0.2m, 0.07m));

        var engine = new FakeStrategyEngine
        {
            OnSimulate = (_, settings) => FakeStrategyEngine.Profitable(settings, 3),
        };

        var consumer = CreateConsumer(engine);

        // Act
        await consumer.Handle(new RunBacktestMessage(backtest.Id), CancellationToken.None);

        // Assert
        var load = engine.Loads.ShouldHaveSingleItem();
        load.MarketId.ShouldBe(backtest.MarketId);
        load.Start.ShouldBe(Start);
        load.End.ShouldBe(End);
        load.Configuration.ShouldBe(configuration);

        var settings = engine.Simulations.ShouldHaveSingleItem();
        settings.Start.ShouldBe(Start);
        settings.End.ShouldBe(End);
        settings.Budget.ShouldBe(1000m);
        settings.ParticipationRate.ShouldBe(0.2m);
        settings.SlippageMultiplier.ShouldBe(0.07m);
        settings.Progress.ShouldNotBeNull();
        engine.SimulatedConfigurations.ShouldHaveSingleItem().ShouldBe(configuration);

        var saved = await LoadAsync(backtest.Id);
        saved.Status.ShouldBe(BacktestStatus.Completed);
        saved.Results.ShouldNotBeNull();
        saved.Results.TotalTrades.ShouldBe(3);
        saved.Results.FinalBalance.ShouldBe(1030m);
        saved.Results.TotalReturn.ShouldBe(30m);
        saved.Results.OptimizedConfiguration.ShouldBeNull();
        saved.Positions.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_WhenLoadFails_ShouldFailBacktest()
    {
        // Arrange
        var backtest = await SeedBacktestAsync(new FlipConfiguration(), new BacktestRunSettings());

        var engine = new FakeStrategyEngine
        {
            OnLoad = _ => throw new InvalidOperationException("No market data."),
        };

        var consumer = CreateConsumer(engine);

        // Act
        await consumer.Handle(new RunBacktestMessage(backtest.Id), CancellationToken.None);

        // Assert
        engine.Simulations.ShouldBeEmpty();
        var saved = await LoadAsync(backtest.Id);
        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldBe("No market data.");
    }

    [Fact]
    public async Task Handle_WhenNoEngineForStrategyMode_ShouldFailBacktest()
    {
        // Arrange
        var backtest = await SeedBacktestAsync(new FlipConfiguration(), new BacktestRunSettings());
        var swingEngine = new FakeStrategyEngine(StrategyMode.Swing);
        var consumer = CreateConsumer(swingEngine);

        // Act
        await consumer.Handle(new RunBacktestMessage(backtest.Id), CancellationToken.None);

        // Assert
        swingEngine.Loads.ShouldBeEmpty();

        var saved = await LoadAsync(backtest.Id);
        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldNotBeNull();
        saved.ErrorMessage.ShouldContain(nameof(StrategyMode.Flip));
    }

    [Fact]
    public async Task Handle_WhenBacktestMissing_ShouldNotLoadData()
    {
        // Arrange
        var engine = new FakeStrategyEngine();
        var consumer = CreateConsumer(engine);

        // Act
        await consumer.Handle(
            new RunBacktestMessage(new Id<Backtest>(Guid.NewGuid().ToString())),
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
                new RunBacktestMessage(new Id<Backtest>(Guid.NewGuid().ToString())),
                new CancellationToken(true)
            );

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<Backtest> SeedBacktestAsync(
        StrategyConfiguration configuration,
        BacktestRunSettings runSettings
    )
    {
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var strategy = Strategy.Create(marketId, "Test", null, configuration);

        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            Start,
            End,
            1000m,
            runSettings,
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
