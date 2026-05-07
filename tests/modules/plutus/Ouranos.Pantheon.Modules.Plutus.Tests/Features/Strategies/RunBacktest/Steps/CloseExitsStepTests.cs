using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class CloseExitsStepTests
{
    private readonly IFixture _fixture = new Fixture();

    public CloseExitsStepTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHoldPeriodExceeded_ClosesPosition()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration(HoldPeriodDays: 1)
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: startDate.AddDays(0)
        );
        var currentDate = startDate.AddDays(1);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 1000m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 1 };
        var step = new CloseExitsStep([]);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.Count.ShouldBe(1);
        payload.Portfolio.Balance.ShouldBeGreaterThan(10000m);

        var closedPosition = payload.Portfolio.ClosedPositions[0];
        closedPosition.SymbolId.ShouldBe(symbolId.ToString());
        closedPosition.EntryPrice.ShouldBe(100m);
        closedPosition.ExitPrice.ShouldBeGreaterThan(100m);
        closedPosition.Volume.ShouldBe(10m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHoldPeriodNotExceeded_KeepsPositionOpen()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration(HoldPeriodDays: 5)
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: startDate.AddDays(0)
        );
        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 1 };
        var step = new CloseExitsStep([]);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
        payload.Portfolio.OpenPositions.ShouldContainKey(symbolId);
        payload.Portfolio.ClosedPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSellThresholdAndScoreBelowThreshold_ClosesPosition()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        executor.Score(Arg.Any<StrategyScoreContext>(), Arg.Any<StrategyConfiguration>())
            .Returns(3m);
        var signalComputer = Substitute.For<ISignalComputer>();
        signalComputer.Type.Returns(SignalType.TaxAdjustedRoi);
        signalComputer.ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(0.5m);
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration(SellThreshold: 5m, HoldPeriodDays: 100)
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: startDate.AddDays(0)
        );
        var currentDate = startDate.AddDays(1);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 1000m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 1 };
        var step = new CloseExitsStep([signalComputer]);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.Count.ShouldBe(1);
        payload.Portfolio.Balance.ShouldBeGreaterThan(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSellThresholdAndScoreAboveThreshold_KeepsPositionOpen()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        executor.Score(Arg.Any<StrategyScoreContext>(), Arg.Any<StrategyConfiguration>())
            .Returns(7m);
        var signalComputer = Substitute.For<ISignalComputer>();
        signalComputer.Type.Returns(SignalType.TaxAdjustedRoi);
        signalComputer.ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(0.5m);
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration(SellThreshold: 5m, HoldPeriodDays: 100)
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: startDate.AddDays(0)
        );
        var currentDate = startDate.AddDays(1);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 1000m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 1 };
        var step = new CloseExitsStep([signalComputer]);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
        payload.Portfolio.OpenPositions.ShouldContainKey(symbolId);
        payload.Portfolio.ClosedPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExitPriceZero_SkipsPosition()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration(HoldPeriodDays: 1)
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: startDate.AddDays(0)
        );

        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 1 };
        var step = new CloseExitsStep([]);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
        payload.Portfolio.OpenPositions.ShouldContainKey(symbolId);
        payload.Portfolio.ClosedPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoOpenPositions_DoesNothing()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration(HoldPeriodDays: 1)
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 1 };
        var step = new CloseExitsStep([]);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPartialExit_ReducesVolume()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration(HoldPeriodDays: 1)
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m,
            VolumeParticipationRate: 0.10m
        );
        var payload = new BacktestPayload(parameters);
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 100m,
            EntryTime: startDate.AddDays(0)
        );
        var currentDate = startDate.AddDays(1);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        // Daily volume = 500 -> maxSellable = floor(500 * 0.10) = 50
        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 500m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 1 };
        var step = new CloseExitsStep([]);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
        payload.Portfolio.OpenPositions.ShouldContainKey(symbolId);
        var remainingPosition = payload.Portfolio.OpenPositions[symbolId];
        remainingPosition.Volume.ShouldBe(50m); // 100 - 50 = 50
        payload.Portfolio.ClosedPositions.Count.ShouldBe(1);
        payload.Portfolio.ClosedPositions[0].Volume.ShouldBe(50m);
        payload.Portfolio.Balance.ShouldBeGreaterThan(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var context = new PipelineContext(cts.Token);
        var step = new CloseExitsStep([]);

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            step.ExecuteAsync(context, payload)
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenContextIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            startDate.AddDays(10),
            10000m
        );
        var payload = new BacktestPayload(parameters);

        var context = new PipelineContext(CancellationToken.None);
        var step = new CloseExitsStep([]);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            step.ExecuteAsync(context, payload)
        );
    }
}
