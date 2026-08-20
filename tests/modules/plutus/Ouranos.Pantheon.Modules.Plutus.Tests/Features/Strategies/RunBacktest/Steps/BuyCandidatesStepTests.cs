using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class BuyCandidatesStepTests
{
    private readonly IFixture _fixture = new Fixture();

    public BuyCandidatesStepTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoScoredSymbols_DoesNotAddPositions()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [symbol], []);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            parameters.StartDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.ScoredSymbols = [];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScoredSymbolsBelowThreshold_DoesNotAddPositions()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            new InputThresholds(BuyThreshold: 10m)
        );
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [symbol], []);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            parameters.StartDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 5m, 100m)];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenScoredSymbolAboveThreshold_AddsPosition()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields(Limit: 1000m)
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var currentDate = startDate.AddDays(0);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 100m, 100m, 100m, 100000m),
        };
        var data = BacktestData.FromRaw(market, [symbol], dailyAggregates);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            startDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 50m, 100m)];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
        payload.Portfolio.OpenPositions.ShouldContainKey(symbolId);
        payload.Portfolio.Balance.ShouldBeLessThan(10000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSymbolAlreadyHeld_SkipsCandidate()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields(Limit: 1000m)
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [symbol], []);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            parameters.StartDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            100m,
            10m,
            DateTimeOffset.UtcNow
        );
        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 50m, 100m)];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInsufficientBalance_SkipsCandidate()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields(Limit: 1000m)
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            1m // very low budget
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [symbol], []);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            parameters.StartDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 50m, 100m)];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(1m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSlippageApplied_AdjustsBuyPrice()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields(Limit: 10000m)
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var currentDate = startDate.AddDays(0);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 100m, 100m, 100m, 100000m),
        };
        var data = BacktestData.FromRaw(market, [symbol], dailyAggregates);
        payload.Context = new BacktestContext(
            data,
            executor,
            0.10m,
            startDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 50m, 100m)];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
        payload.Portfolio.OpenPositions[symbolId].EntryPrice.ShouldBe(100.01m);
        payload.Portfolio.Balance.ShouldBe(99.01m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScoreJustAboveZeroAndBuyThresholdNull_BuysCandidate()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields(Limit: 1000m)
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var currentDate = startDate.AddDays(0);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 100m, 100m, 100m, 100000m),
        };
        var data = BacktestData.FromRaw(market, [symbol], dailyAggregates);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            startDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 0.01m, 100m)];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);
        payload.Portfolio.OpenPositions.ShouldContainKey(symbolId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVolumeExceedsDailyParticipation_CapsVolume()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields(Limit: 10000m)
        );
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m,
            VolumeParticipationRate: 0.1m
        );
        var payload = new BacktestPayload(parameters);
        var currentDate = startDate.AddDays(0);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        // Low daily volume so participation rate caps the buy volume
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 100m, 100m, 100m, 50m),
        };
        var data = BacktestData.FromRaw(market, [symbol], dailyAggregates);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            startDate,
            parameters.InputWeights,
            parameters.Thresholds
        );
        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 50m, 100m)];

        var context = new PipelineContext(CancellationToken.None);
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.OpenPositions.Count.ShouldBe(1);

        var position = payload.Portfolio.OpenPositions[symbolId];
        position.Volume.ShouldBe(5m);
    }
}
