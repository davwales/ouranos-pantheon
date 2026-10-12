using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
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
        dailyAggregates.InsertRange(
            0,
            BacktestTestData.History(symbolId, currentDate, 100m, 100000m)
        );
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
            10000m,
            VolumeParticipationRate: 0.25m,
            SlippageMultiplier: 0.1m
        );
        var payload = new BacktestPayload(parameters);
        var currentDate = startDate.AddDays(0);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 100m, 100m, 100m, 100000m),
        };
        dailyAggregates.InsertRange(
            0,
            BacktestTestData.History(symbolId, currentDate, 100m, 100000m)
        );
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
        var position = payload.Portfolio.OpenPositions[symbolId];
        position.EntryPrice.ShouldBe(100.3162m, 0.0001m);
        position.Volume.ShouldBe(99m);
        payload.Portfolio.Balance.ShouldBe(68.6935m, 0.001m);
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
        dailyAggregates.InsertRange(
            0,
            BacktestTestData.History(symbolId, currentDate, 100m, 100000m)
        );
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
            VolumeParticipationRate: 0.1m,
            MinFillFraction: 0m
        );
        var payload = new BacktestPayload(parameters);
        var currentDate = startDate.AddDays(0);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        // Low daily volume so participation rate caps the buy volume
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 100m, 100m, 100m, 50m),
        };
        dailyAggregates.InsertRange(0, BacktestTestData.History(symbolId, currentDate, 100m, 50m));
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

    [Fact]
    public async Task ExecuteAsync_WhenSymbolDidNotTradeToday_DoesNotBuy()
    {
        // Arrange
        var (payload, symbol, _) = CreateLiquidityScenario(
            todayVolume: null,
            historyVolume: 1000m,
            limit: null
        );

        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 1m, 100m)];
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(new PipelineContext(CancellationToken.None), payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
        payload.Portfolio.Balance.ShouldBe(100_000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypicalVolumeIsThin_DoesNotBuyOnSingleBusyDay()
    {
        // Arrange
        var (payload, symbol, _) = CreateLiquidityScenario(
            todayVolume: 100_000m,
            historyVolume: 1m,
            limit: null
        );

        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 1m, 100m)];
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(new PipelineContext(CancellationToken.None), payload);

        // Assert
        payload.Portfolio.OpenPositions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSymbolHasBuyLimit_CapsVolumeAtLimitWindowsPerDay()
    {
        // Arrange
        var (payload, symbol, symbolId) = CreateLiquidityScenario(
            todayVolume: 100_000m,
            historyVolume: 100_000m,
            limit: 70m
        );

        payload.Portfolio.ScoredSymbols = [new ScoredSymbol(symbol, 1m, 100m)];
        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(new PipelineContext(CancellationToken.None), payload);

        // Assert
        payload
            .Portfolio.OpenPositions[symbolId]
            .Volume.ShouldBe(70m * BacktestDataOptions.DefaultLimitWindowsPerDay);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCappedFillIsBelowMinFillFraction_SkipsAndBuysNextCandidate()
    {
        // Arrange
        var (payload, thinSymbol, thinId) = CreateLiquidityScenario(
            todayVolume: 40m,
            historyVolume: 40m,
            limit: null,
            new TradingConfiguration(MaxPositions: 1, MaxPositionPercent: 0.5m)
        );

        var liquidId = _fixture.Create<Id<Symbol>>();
        var liquidSymbol = Symbol.Create(
            liquidId,
            "LIQ",
            null,
            "Liquid Symbol",
            thinSymbol.MarketId,
            new AdditionalFields()
        );

        var aggregates = BacktestTestData.History(
            liquidId,
            payload.Parameters.StartDate,
            100m,
            100_000m
        );

        aggregates.Add(
            new DailyTradeAggregate(
                liquidId,
                DateOnly.FromDateTime(payload.Parameters.StartDate.UtcDateTime),
                100m,
                100m,
                100m,
                100_000m
            )
        );

        var thinAggregates = payload.Context.ShouldNotBeNull().Data.AggregatesBySymbol[thinId];
        payload.Context = payload.Context with
        {
            Data = BacktestData.FromRaw(
                payload.Context.Data.Market,
                [thinSymbol, liquidSymbol],
                [.. thinAggregates, .. aggregates]
            ),
        };

        payload.Portfolio.ScoredSymbols =
        [
            new ScoredSymbol(thinSymbol, 0.9m, 100m),
            new ScoredSymbol(liquidSymbol, 0.5m, 100m),
        ];

        var step = new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>());

        // Act
        await step.ExecuteAsync(new PipelineContext(CancellationToken.None), payload);

        // Assert
        payload.Portfolio.OpenPositions.Keys.ShouldBe([liquidId]);
    }

    private (BacktestPayload Payload, Symbol Symbol, Id<Symbol> SymbolId) CreateLiquidityScenario(
        decimal? todayVolume,
        decimal historyVolume,
        decimal? limit,
        TradingConfiguration? configuration = null
    )
    {
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);

        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields(Limit: limit)
        );

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            configuration ?? new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );

        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            100_000m
        );

        var aggregates = BacktestTestData.History(symbolId, startDate, 100m, historyVolume);
        if (todayVolume.HasValue)
        {
            aggregates.Add(
                new DailyTradeAggregate(
                    symbolId,
                    DateOnly.FromDateTime(startDate.UtcDateTime),
                    100m,
                    100m,
                    100m,
                    todayVolume.Value
                )
            );
        }

        var payload = new BacktestPayload(parameters)
        {
            Context = new BacktestContext(
                BacktestData.FromRaw(market, [symbol], aggregates),
                Substitute.For<IStrategyExecutor>(),
                0m,
                startDate,
                parameters.InputWeights,
                parameters.Thresholds
            ),
        };

        return (payload, symbol, symbolId);
    }
}
