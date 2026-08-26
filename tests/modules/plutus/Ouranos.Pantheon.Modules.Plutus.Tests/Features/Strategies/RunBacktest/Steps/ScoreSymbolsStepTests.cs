using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class ScoreSymbolsStepTests
{
    private readonly IFixture _fixture = new Fixture();

    public ScoreSymbolsStepTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoSymbols_SetsEmptyScoredSymbols()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
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
        var data = BacktestData.FromRaw(market, [], []);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            parameters.StartDate,
            parameters.InputWeights,
            parameters.Thresholds
        );

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.ScoredSymbols.ShouldNotBeNull();
        payload.Portfolio.ScoredSymbols.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSymbolHasNoAggregates_SkipsSymbol()
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

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.ScoredSymbols.ShouldNotBeNull();
        payload.Portfolio.ScoredSymbols.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecutorReturnsNullScore_SkipsSymbol()
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
        executor
            .Score(Arg.Any<StrategyScoreContext>(), Arg.Any<TradingConfiguration>())
            .Returns((decimal?)null);
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
            new(symbolId, dateOnly, 100m, 95m, 105m, 10000m),
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

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.ScoredSymbols.ShouldNotBeNull();
        payload.Portfolio.ScoredSymbols.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecutorReturnsScore_AddsScoredSymbol()
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
        executor
            .Score(Arg.Any<StrategyScoreContext>(), Arg.Any<TradingConfiguration>())
            .Returns(75m);
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
            new(symbolId, dateOnly, 100m, 95m, 105m, 10000m),
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

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.ScoredSymbols.ShouldNotBeNull();
        payload.Portfolio.ScoredSymbols.Count.ShouldBe(1);
        payload.Portfolio.ScoredSymbols[0].Symbol.Id.ShouldBe(symbolId);
        payload.Portfolio.ScoredSymbols[0].Score.ShouldBe(75m);
        payload.Portfolio.ScoredSymbols[0].Price.ShouldBe(100m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleSymbols_ScoresAllThatHaveData()
    {
        // Arrange
        var symbolId1 = _fixture.Create<Id<Symbol>>();
        var symbolId2 = _fixture.Create<Id<Symbol>>();
        var symbolId3 = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol1 = Symbol.Create(
            symbolId1,
            "SYM1",
            null,
            "Symbol One",
            marketId,
            new AdditionalFields()
        );
        var symbol2 = Symbol.Create(
            symbolId2,
            "SYM2",
            null,
            "Symbol Two",
            marketId,
            new AdditionalFields()
        );
        var symbol3 = Symbol.Create(
            symbolId3,
            "SYM3",
            null,
            "Symbol Three",
            marketId,
            new AdditionalFields()
        );
        var executor = Substitute.For<IStrategyExecutor>();
        executor
            .Score(Arg.Any<StrategyScoreContext>(), Arg.Any<TradingConfiguration>())
            .Returns(80m);

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
            new(symbolId1, dateOnly, 100m, 95m, 105m, 10000m),
            new(symbolId3, dateOnly, 50m, 48m, 52m, 5000m),
        };
        var data = BacktestData.FromRaw(market, [symbol1, symbol2, symbol3], dailyAggregates);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            startDate,
            parameters.InputWeights,
            parameters.Thresholds
        );

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.ScoredSymbols.ShouldNotBeNull();
        payload.Portfolio.ScoredSymbols.Count.ShouldBe(2);
        payload.Portfolio.ScoredSymbols.ShouldContain(s => s.Symbol.Id == symbolId1);
        payload.Portfolio.ScoredSymbols.ShouldContain(s => s.Symbol.Id == symbolId3);
        payload.Portfolio.ScoredSymbols.ShouldNotContain(s => s.Symbol.Id == symbolId2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSignalComputersReturnValues_IncludeSignalsInScoreContext()
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
        executor
            .Score(Arg.Any<StrategyScoreContext>(), Arg.Any<TradingConfiguration>())
            .Returns(90m);

        var signalComputer = Substitute.For<ISignalComputer>();
        signalComputer.Type.Returns(SignalType.TrendMomentum);
        signalComputer
            .ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(0.85m);

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
            new(symbolId, dateOnly, 100m, 95m, 105m, 10000m),
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

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([signalComputer]));

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        await signalComputer
            .Received(1)
            .ComputeAsync(
                Arg.Is<SignalComputeContext>(c => c.SymbolId == symbolId),
                Arg.Any<CancellationToken>()
            );

        payload.Portfolio.ScoredSymbols.ShouldNotBeNull();
        payload.Portfolio.ScoredSymbols.Count.ShouldBe(1);

        executor
            .Received(1)
            .Score(
                Arg.Is<StrategyScoreContext>(ctx =>
                    ctx.SymbolId == symbolId
                    && ctx.Signals.Count == 1
                    && ctx.Signals[0].Type == SignalType.TrendMomentum
                    && ctx.Signals[0].Value == 0.85m
                    && ctx.Signals[0].SymbolId == symbolId
                ),
                Arg.Any<TradingConfiguration>()
            );
    }

    [Fact]
    public async Task ExecuteAsync_AccumulatesSignalHistoryAcrossIterationsAndPassesToScoreContext()
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

        var capturedContexts = new List<StrategyScoreContext>();
        var executor = Substitute.For<IStrategyExecutor>();
        executor
            .Score(Arg.Any<StrategyScoreContext>(), Arg.Any<TradingConfiguration>())
            .Returns(50m)
            .AndDoes(ci => capturedContexts.Add(ci.Arg<StrategyScoreContext>()));

        var signalComputer = Substitute.For<ISignalComputer>();
        signalComputer.Type.Returns(SignalType.TrendMomentum);
        signalComputer
            .ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(0.85m);

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

        var dateOnly0 = DateOnly.FromDateTime(startDate.UtcDateTime);
        var dateOnly1 = DateOnly.FromDateTime(startDate.AddDays(1).UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly0, 100m, 95m, 105m, 10000m),
            new(symbolId, dateOnly1, 102m, 97m, 107m, 10000m),
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

        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([signalComputer]));

        // Act
        await step.ExecuteAsync(new PipelineContext(CancellationToken.None), payload);

        await step.ExecuteAsync(
            new PipelineContext(CancellationToken.None) { CurrentIteration = 1 },
            payload
        );

        // Assert
        capturedContexts.Count.ShouldBe(2);

        capturedContexts[0].SignalHistoryByType.ShouldBeNull();

        var history = capturedContexts[1].SignalHistoryByType;
        history.ShouldNotBeNull();
        history.ShouldContainKey(SignalType.TrendMomentum);
        history[SignalType.TrendMomentum].ShouldBe([0.85m]);

        payload.SignalHistoryBuffer.ShouldContainKey(symbolId);
        payload.SignalHistoryBuffer[symbolId][SignalType.TrendMomentum].ShouldBe([0.85m, 0.85m]);
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
        var data = BacktestData.FromRaw(market, [], []);
        payload.Context = new BacktestContext(
            data,
            executor,
            0m,
            parameters.StartDate,
            parameters.InputWeights,
            parameters.Thresholds
        );

        var context = new PipelineContext(new CancellationToken(true));
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

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

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => step.ExecuteAsync(context, payload));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCurrentPriceIsZero_SkipsSymbol()
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
            new(symbolId, dateOnly, 0m, 0m, 0m, 0m),
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

        var context = new PipelineContext(CancellationToken.None);
        var logger = Substitute.For<ILogger<ScoreSymbolsStep>>();
        var step = new ScoreSymbolsStep(logger, new SignalScoringService([]));

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.ScoredSymbols.ShouldNotBeNull();
        payload.Portfolio.ScoredSymbols.ShouldBeEmpty();

        executor
            .DidNotReceiveWithAnyArgs()
            .Score(Arg.Any<StrategyScoreContext>(), Arg.Any<TradingConfiguration>());
    }
}
