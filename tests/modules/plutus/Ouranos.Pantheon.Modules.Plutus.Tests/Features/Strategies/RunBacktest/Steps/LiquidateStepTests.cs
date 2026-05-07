using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class LiquidateStepTests
{
    private readonly IFixture _fixture = new Fixture();

    public LiquidateStepTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFullExitAvailable_ClosesPositionAndAddsProceeds()
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
            new StrategyConfiguration()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var endDate = DateTimeOffset.UtcNow;
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            endDate,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var dateOnly = DateOnly.FromDateTime(endDate.UtcDateTime);

        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 1000m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var position = new OpenPosition(symbolId, "SYM", null, 100m, 10m, startDate);
        payload.Portfolio.OpenPositions[symbolId] = position;

        var context = new PipelineContext(CancellationToken.None);
        var step = new LiquidateStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.Count.ShouldBe(1);
        payload.Portfolio.Balance.ShouldBe(10000m + 1498.5m);

        var closedPosition = payload.Portfolio.ClosedPositions[0];
        closedPosition.SymbolId.ShouldBe(symbolId.ToString());
        closedPosition.SymbolName.ShouldBe("SYM");
        closedPosition.EntryPrice.ShouldBe(100m);
        closedPosition.ExitPrice.ShouldBe(150m);
        closedPosition.Volume.ShouldBe(10m);
        closedPosition.ProfitLoss.ShouldBe(498.5m);
        closedPosition.EntryTime.ShouldBe(startDate);
        closedPosition.ExitTime.ShouldBe(endDate);
    }

    [Fact]
    public async Task ExecuteAsync_WhenZeroExitPrice_UsesEntryPriceAsFallback()
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
            new StrategyConfiguration()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var endDate = DateTimeOffset.UtcNow;
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            endDate,
            10000m
        );
        var payload = new BacktestPayload(parameters);

        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var position = new OpenPosition(symbolId, "SYM", null, 100m, 10m, startDate);
        payload.Portfolio.OpenPositions[symbolId] = position;

        var context = new PipelineContext(CancellationToken.None);
        var step = new LiquidateStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.Count.ShouldBe(1);
        payload.Portfolio.Balance.ShouldBe(11000m);

        var closedPosition = payload.Portfolio.ClosedPositions[0];
        closedPosition.ExitPrice.ShouldBe(100m);
        closedPosition.ProfitLoss.ShouldBe(0m);

        closedPosition.ExitPrice.ShouldNotBe(50m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExitVolumeZero_ForceLiquidatesAtHalfEntryPrice()
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
            new StrategyConfiguration()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var endDate = DateTimeOffset.UtcNow;
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            endDate,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var dateOnly = DateOnly.FromDateTime(endDate.UtcDateTime);

        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 1m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var position = new OpenPosition(symbolId, "SYM", null, 100m, 10m, startDate);
        payload.Portfolio.OpenPositions[symbolId] = position;

        var context = new PipelineContext(CancellationToken.None);
        var step = new LiquidateStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.Count.ShouldBe(1);
        payload.Portfolio.Balance.ShouldBe(10000m + 500m);

        var closedPosition = payload.Portfolio.ClosedPositions[0];
        closedPosition.ExitPrice.ShouldBe(50m);
        closedPosition.Volume.ShouldBe(10m);
        closedPosition.ProfitLoss.ShouldBe(-500m);
        closedPosition.EntryPrice.ShouldBe(100m);
        closedPosition.SymbolId.ShouldBe(symbolId.ToString());
        closedPosition.SymbolName.ShouldBe("SYM");
    }

    [Fact]
    public async Task ExecuteAsync_WhenExitVolumeZeroWithTax_AppliesTaxCapToForcedLiquidation()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(new FlatTax(0m, 100m, 0.10m)));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var endDate = DateTimeOffset.UtcNow;
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            endDate,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var dateOnly = DateOnly.FromDateTime(endDate.UtcDateTime);

        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 1m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0.10m, 7, startDate);

        var position = new OpenPosition(symbolId, "SYM", null, 100m, 10m, startDate);
        payload.Portfolio.OpenPositions[symbolId] = position;

        var context = new PipelineContext(CancellationToken.None);
        var step = new LiquidateStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.Count.ShouldBe(1);
        payload.Portfolio.Balance.ShouldBe(10000m + 450m);

        var closedPosition = payload.Portfolio.ClosedPositions[0];
        closedPosition.ExitPrice.ShouldBe(50m);
        closedPosition.ProfitLoss.ShouldBe(-550m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPartialExit_ForceLiquidatesRemainingAtHalfEntryPrice()
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
            new StrategyConfiguration()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var endDate = DateTimeOffset.UtcNow;
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            endDate,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var dateOnly = DateOnly.FromDateTime(endDate.UtcDateTime);

        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 150m, 150m, 150m, 20m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);

        var position = new OpenPosition(symbolId, "SYM", null, 100m, 10m, startDate);
        payload.Portfolio.OpenPositions[symbolId] = position;

        var context = new PipelineContext(CancellationToken.None);
        var step = new LiquidateStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.Count.ShouldBe(2);
        payload.Portfolio.Balance.ShouldBe(10000m + 731.25m + 250m);

        var firstClosed = payload.Portfolio.ClosedPositions[0];
        firstClosed.ExitPrice.ShouldBe(150m);
        firstClosed.Volume.ShouldBe(5m);
        firstClosed.ProfitLoss.ShouldBe(231.25m);
        firstClosed.EntryPrice.ShouldBe(100m);

        var secondClosed = payload.Portfolio.ClosedPositions[1];
        secondClosed.ExitPrice.ShouldBe(50m);
        secondClosed.Volume.ShouldBe(5m);
        secondClosed.ProfitLoss.ShouldBe(-250m);
        secondClosed.EntryPrice.ShouldBe(100m);
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
            new StrategyConfiguration()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var endDate = DateTimeOffset.UtcNow;
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            endDate,
            10000m
        );
        var payload = new BacktestPayload(parameters)
        {
            Context = new BacktestContext(
                BacktestData.FromRaw(market, [], [], [], [], []),
                executor,
                0m,
                7,
                startDate
            )
        };

        var context = new PipelineContext(CancellationToken.None);
        var step = new LiquidateStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.ClosedPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(10000m);
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
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters)
        {
            Context = new BacktestContext(
                BacktestData.FromRaw(market, [], [], [], [], []),
                executor,
                0m,
                7,
                parameters.StartDate
            )
        };

        var cancelledCts = new CancellationTokenSource();
        await cancelledCts.CancelAsync();
        var context = new PipelineContext(cancelledCts.Token);
        var step = new LiquidateStep();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() => step.ExecuteAsync(context, payload));
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
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var context = new PipelineContext(CancellationToken.None);
        var step = new LiquidateStep();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => step.ExecuteAsync(context, payload));
    }
}
