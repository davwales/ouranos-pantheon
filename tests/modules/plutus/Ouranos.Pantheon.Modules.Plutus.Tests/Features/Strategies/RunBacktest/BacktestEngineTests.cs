using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class BacktestEngineTests
{
    private readonly IFixture _fixture = new Fixture();

    public BacktestEngineTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(15, 1)]
    [InlineData(30, 1)]
    [InlineData(31, 3)]
    [InlineData(60, 3)]
    [InlineData(90, 3)]
    [InlineData(91, 7)]
    [InlineData(180, 7)]
    [InlineData(365, 7)]
    [InlineData(366, 14)]
    [InlineData(730, 14)]
    public void DetermineWindowSize_WhenGivenTotalDays_ReturnsCorrectWindow(
        int totalDays,
        int expectedWindow
    )
    {
        // Arrange - handled by InlineData

        // Act
        var result = BacktestEngine.DetermineWindowSize(totalDays);

        // Assert
        result.ShouldBe(expectedWindow);
    }

    [Fact]
    public void DetermineWindowSize_WhenZeroDays_ReturnsOneDayWindow()
    {
        // Arrange
        const int totalDays = 0;

        // Act
        var result = BacktestEngine.DetermineWindowSize(totalDays);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public void GetTaxRate_WhenMarketHasFlatTax_ReturnsRate()
    {
        // Arrange
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            "Test Market",
            new Taxes(new FlatTax(0, 500, 0.10m))
        );

        // Act
        var result = BacktestEngine.GetTaxRate(market);

        // Assert
        result.ShouldBe(0.10m);
    }

    [Fact]
    public void GetTaxRate_WhenMarketHasNoFlatTax_ReturnsZero()
    {
        // Arrange
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            "Test Market",
            new Taxes(null)
        );

        // Act
        var result = BacktestEngine.GetTaxRate(market);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void ComputeExit_WhenProfitableSale_ReturnsPositivePnl()
    {
        // Arrange
        var pos = new OpenPosition(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: DateTimeOffset.UtcNow
        );
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            "Test Market",
            new Taxes(new FlatTax(0, 1000m, 0.05m))
        );

        // Act
        var (netProceeds, exitVolume, netPnl) = BacktestEngine.ComputeExit(pos, 150m, 0.05m, market);

        // Assert
        exitVolume.ShouldBe(10m);
        netProceeds.ShouldBe(1425m);
        netPnl.ShouldBe(425m);
    }

    [Fact]
    public void ComputeExit_WhenTaxExceedsCap_AppliesTaxCap()
    {
        // Arrange
        var pos = new OpenPosition(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 100m,
            EntryTime: DateTimeOffset.UtcNow
        );
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            "Test Market",
            new Taxes(new FlatTax(0, 50m, 0.10m))
        );

        // Act
        var (netProceeds, _, netPnl) = BacktestEngine.ComputeExit(pos, 200m, 0.10m, market);

        // Assert
        netProceeds.ShouldBe(19950m);
        netPnl.ShouldBe(9950m);
    }

    [Fact]
    public void ComputeExit_WhenLoss_ReturnsNegativePnl()
    {
        // Arrange
        var pos = new OpenPosition(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: DateTimeOffset.UtcNow
        );
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            "Test Market",
            new Taxes(new FlatTax(0, 1000m, 0.05m))
        );

        // Act
        var (_, _, netPnl) = BacktestEngine.ComputeExit(pos, 50m, 0.05m, market);

        // Assert
        netPnl.ShouldBe(-525m);
    }

    [Fact]
    public void ComputeExit_WhenNoTax_ReturnsFullGross()
    {
        // Arrange
        var pos = new OpenPosition(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            EntryPrice: 50m,
            Volume: 20m,
            EntryTime: DateTimeOffset.UtcNow
        );
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            "Test Market",
            new Taxes(null)
        );

        // Act
        var (netProceeds, _, netPnl) = BacktestEngine.ComputeExit(pos, 60m, 0m, market);

        // Assert
        netProceeds.ShouldBe(1200m);
        netPnl.ShouldBe(200m);
    }

    [Fact]
    public void GetSnapshotsForSymbol_WhenSnapshotsExistForAllTimeFrames_ReturnsAllThree()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var shortSnap = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            1000m,
            90m,
            110m,
            1000m,
            10,
            1000m,
            1m
        );
        var mediumSnap = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneWeek,
            5000m,
            80m,
            120m,
            5000m,
            50,
            1000m,
            5m
        );
        var longSnap = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneMonth,
            20000m,
            70m,
            130m,
            20000m,
            200,
            1000m,
            20m
        );
        var allSnapshots = new List<MarketTradeSnapshot> { shortSnap, mediumSnap, longSnap };

        var data = BacktestData.FromRaw(
            market,
            [],
            allSnapshots,
            [],
            [],
            [],
            []
        );

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(symbolId);

        // Assert
        shortResult.ShouldNotBeNull();
        mediumResult.ShouldNotBeNull();
        longResult.ShouldNotBeNull();
        shortResult.TimeFrame.ShouldBe(TimeFrame.OneHour);
        mediumResult.TimeFrame.ShouldBe(TimeFrame.OneWeek);
        longResult.TimeFrame.ShouldBe(TimeFrame.OneMonth);
    }

    [Fact]
    public void GetSnapshotsForSymbol_WhenNoSnapshotsForSymbol_ReturnsAllNull()
    {
        // Arrange
        var targetSymbolId = _fixture.Create<Id<Symbol>>();
        var otherSymbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var snap = MarketTradeSnapshot.Create(
            marketId,
            otherSymbolId,
            TimeFrame.OneHour,
            1000m,
            90m,
            110m,
            1000m,
            10,
            1000m,
            1m
        );
        var allSnapshots = new List<MarketTradeSnapshot> { snap };

        var data = BacktestData.FromRaw(
            market,
            [],
            allSnapshots,
            [],
            [],
            [],
            []
        );

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(targetSymbolId);

        // Assert
        shortResult.ShouldBeNull();
        mediumResult.ShouldBeNull();
        longResult.ShouldBeNull();
    }

    [Fact]
    public void GetSnapshotsForSymbol_WhenEmptyList_ReturnsAllNull()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var allSnapshots = new List<MarketTradeSnapshot>();

        var data = BacktestData.FromRaw(
            market,
            [],
            allSnapshots,
            [],
            [],
            [],
            []
        );

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(symbolId);

        // Assert
        shortResult.ShouldBeNull();
        mediumResult.ShouldBeNull();
        longResult.ShouldBeNull();
    }

    [Fact]
    public void GetSignalsForSymbol_WhenSignalsExist_ReturnsMatchingSignals()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var signal1 = Signal.Create(marketId, symbolId, SignalType.TaxAdjustedRoi, 0.8m);
        var signal2 = Signal.Create(marketId, symbolId, SignalType.TrendMomentum, 0.5m);
        var otherSymbolId = _fixture.Create<Id<Symbol>>();
        var signal3 = Signal.Create(marketId, otherSymbolId, SignalType.Rsi, 0.3m);
        var allSignals = new List<Signal> { signal1, signal2, signal3 };

        var data = BacktestData.FromRaw(
            market,
            [],
            [],
            [],
            allSignals,
            [],
            []
        );

        // Act
        var result = data.GetSignalsForSymbol(symbolId);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(s => s.Type == SignalType.TaxAdjustedRoi);
        result.ShouldContain(s => s.Type == SignalType.TrendMomentum);
    }

    [Fact]
    public void GetSignalsForSymbol_WhenNoSignalsMatch_ReturnsEmptyList()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var allSignals = new List<Signal>();

        var data = BacktestData.FromRaw(
            market,
            [],
            [],
            [],
            allSignals,
            [],
            []
        );

        // Act
        var result = data.GetSignalsForSymbol(symbolId);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetForecastData_WhenForecastIsNull_ReturnsNulls()
    {
        // Arrange
        const decimal currentPrice = 100m;

        // Act
        var (price, change) = BacktestEngine.GetForecastData(null, currentPrice);

        // Assert
        price.ShouldBeNull();
        change.ShouldBeNull();
    }

    [Fact]
    public void GetForecastData_WhenCurrentPriceIsZero_ReturnsNulls()
    {
        // Arrange
        var forecast = Forecast.Create(
            _fixture.Create<Id<Forecast>>(),
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            new ForecastPoint(110m, 100m, 120m, 500m),
            [new ForecastPoint(105m, 95m, 115m, 400m)]
        );

        // Act
        var (price, change) = BacktestEngine.GetForecastData(forecast, 0m);

        // Assert
        price.ShouldBeNull();
        change.ShouldBeNull();
    }

    [Fact]
    public void GetForecastData_WhenForecastedPriceIsZero_ReturnsPriceWithNullChange()
    {
        // Arrange
        var forecast = Forecast.Create(
            _fixture.Create<Id<Forecast>>(),
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            new ForecastPoint(0m, 0m, 0m, 0m),
            [new ForecastPoint(0m, 0m, 0m, 0m)]
        );

        // Act
        var (price, change) = BacktestEngine.GetForecastData(forecast, 100m);

        // Assert
        price.ShouldBe(0m);
        change.ShouldBeNull();
    }

    [Fact]
    public void GetForecastData_WhenValidForecast_ReturnsPriceAndChange()
    {
        // Arrange
        var forecast = Forecast.Create(
            _fixture.Create<Id<Forecast>>(),
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            new ForecastPoint(110m, 100m, 120m, 500m),
            [new ForecastPoint(105m, 95m, 115m, 400m)]
        );
        const decimal currentPrice = 100m;

        // Act
        var (price, change) = BacktestEngine.GetForecastData(forecast, currentPrice);

        // Assert
        price.ShouldBe(110m);
        change.ShouldBe(0.1m); // (110 - 100) / 100
    }

    [Fact]
    public void GetForecastData_WhenForecastedPriceDeclines_ReturnsNegativeChange()
    {
        // Arrange
        var forecast = Forecast.Create(
            _fixture.Create<Id<Forecast>>(),
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            new ForecastPoint(90m, 80m, 100m, 500m),
            [new ForecastPoint(95m, 85m, 105m, 400m)]
        );
        const decimal currentPrice = 100m;

        // Act
        var (price, change) = BacktestEngine.GetForecastData(forecast, currentPrice);

        // Assert
        price.ShouldBe(90m);
        change.ShouldBe(-0.1m); // (90 - 100) / 100
    }

    [Fact]
    public void BuildPriceBucketsFromAggregates_WhenEmptyAggregates_ReturnsEmptyList()
    {
        // Arrange
        var aggregates = new List<DailyTradeAggregate>();

        // Act
        var result = BacktestEngine.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildPriceBucketsFromAggregates_WhenSingleAggregate_ReturnsOneBucket()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var aggregates = new List<DailyTradeAggregate>
        {
            new DailyTradeAggregate(
                symbolId,
                new DateOnly(2025, 1, 15),
                100m,
                100m,
                100m,
                50m
            )
        };

        // Act
        var result = BacktestEngine.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.Count.ShouldBe(1);
        result[0].AveragePrice.ShouldBe(100m);
        result[0].MinPrice.ShouldBe(100m);
        result[0].MaxPrice.ShouldBe(100m);
        result[0].Volume.ShouldBe(50m);
    }

    [Fact]
    public void BuildPriceBucketsFromAggregates_WhenMultipleAggregates_AggregatesIntoBuckets()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var aggregates = Enumerable.Range(0, 50)
            .Select(i => new DailyTradeAggregate(
                    symbolId,
                    new DateOnly(2025, 1, 1).AddDays(i),
                    100m + i,
                    100m + i,
                    100m + i,
                    10m + i
                )
            )
            .ToList();

        // Act
        var result = BacktestEngine.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.ShouldNotBeEmpty();
        foreach (var bucket in result)
        {
            bucket.MinPrice.ShouldBeLessThanOrEqualTo(bucket.MaxPrice);
            bucket.AveragePrice.ShouldBeGreaterThanOrEqualTo(bucket.MinPrice);
            bucket.AveragePrice.ShouldBeLessThanOrEqualTo(bucket.MaxPrice);
        }
    }

    [Fact]
    public void BuildPriceBucketsFromAggregates_WhenFewerThan25Aggregates_CreatesSmallBuckets()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var aggregates = Enumerable.Range(0, 10)
            .Select(i => new DailyTradeAggregate(
                    symbolId,
                    new DateOnly(2025, 1, 1).AddDays(i),
                    100m + i,
                    100m + i,
                    100m + i,
                    10m
                )
            )
            .ToList();

        // Act
        var result = BacktestEngine.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.Count.ShouldBe(10);
    }

    [Fact]
    public void BuildPriceBucketsFromAggregates_WhenExactly25Aggregates_CreatesExactly25Buckets()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var aggregates = Enumerable.Range(0, 25)
            .Select(i => new DailyTradeAggregate(
                    symbolId,
                    new DateOnly(2025, 1, 1).AddDays(i),
                    100m + i,
                    100m + i,
                    100m + i,
                    10m
                )
            )
            .ToList();

        // Act
        var result = BacktestEngine.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.Count.ShouldBe(25);
        foreach (var bucket in result)
        {
            bucket.MinPrice.ShouldBeLessThanOrEqualTo(bucket.MaxPrice);
        }
    }

    [Fact]
    public void BuyCandidates_WhenSingleTradeAtExactBudget_CalculatesVolumeCorrectly()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );

        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.5m, 100m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 5, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(10000m);

        // Act
        BacktestEngine.BuyCandidates(scoredSymbols, config, 0m, state, DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        state.OpenPositions.Count.ShouldBe(1);
        state.OpenPositions[symbolId].Volume.ShouldBe(100m);
        state.Balance.ShouldBe(0m);
    }

    [Fact]
    public void BuyCandidates_WhenMaxPositionPercentLimitsBudget_RespectsLimit()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.5m, 100m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 5, MaxPositionPercent = 0.5m };
        var state = new BacktestLoopState(10000m);

        // Act
        BacktestEngine.BuyCandidates(scoredSymbols, config, 0m, state, DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        state.OpenPositions.Count.ShouldBe(1);
        state.OpenPositions[symbolId].Volume.ShouldBe(50m);
        state.Balance.ShouldBe(5000m);
    }

    [Fact]
    public void BuyCandidates_WhenScoreAboveThreshold_BuysPosition()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.5m, 100m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 5, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(10000m);

        // Act
        BacktestEngine.BuyCandidates(scoredSymbols, config, 0m, state, DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        state.OpenPositions.Count.ShouldBe(1);
        state.Balance.ShouldBeLessThan(10000m);
    }

    [Fact]
    public void BuyCandidates_WhenScoreBelowThreshold_DoesNotBuy()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.05m, 100m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 5, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(10000m);

        // Act
        BacktestEngine.BuyCandidates(scoredSymbols, config, 0m, state, DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        state.OpenPositions.Count.ShouldBe(0);
        state.Balance.ShouldBe(10000m);
    }

    [Fact]
    public void BuyCandidates_WhenSymbolAlreadyHeld_DoesNotBuyAgain()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.5m, 100m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 5, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(10000m)
        {
            OpenPositions =
            {
                [symbolId] = new OpenPosition(symbolId, "SYM", null, 100m, 5m, DateTimeOffset.UtcNow)
            }
        };

        // Act
        BacktestEngine.BuyCandidates(scoredSymbols, config, 0m, state, DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        state.OpenPositions.Count.ShouldBe(1);
        state.Balance.ShouldBe(10000m);
    }

    [Fact]
    public void BuyCandidates_WhenMaxPositionsReached_DoesNotBuy()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.5m, 100m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 1, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(10000m);
        var existingSymbolId = _fixture.Create<Id<Symbol>>();
        state.OpenPositions[existingSymbolId] = new OpenPosition(
            existingSymbolId,
            "OTHER",
            null,
            50m,
            10m,
            DateTimeOffset.UtcNow
        );

        // Act
        BacktestEngine.BuyCandidates(scoredSymbols, config, 0m, state, DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        state.OpenPositions.Count.ShouldBe(1);
        state.OpenPositions.ShouldContainKey(existingSymbolId);
    }

    [Fact]
    public void BuyCandidates_WhenInsufficientBalance_DoesNotBuy()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.5m, 1000m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 5, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(5m); // Not enough to buy even one share at 1000m + tax

        // Act
        BacktestEngine.BuyCandidates(
            scoredSymbols,
            config,
            0.10m,
            state,
            DateTimeOffset.UtcNow,
            CancellationToken.None
        );

        // Assert
        state.OpenPositions.Count.ShouldBe(0);
    }

    [Fact]
    public void BuyCandidates_WithTax_AdjustsBuyingPower()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)> { (symbol, 0.5m, 100m) };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 5, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(1000m);
        const decimal taxRate = 0.10m;

        // Act
        BacktestEngine.BuyCandidates(
            scoredSymbols,
            config,
            taxRate,
            state,
            DateTimeOffset.UtcNow,
            CancellationToken.None
        );

        // Assert
        state.OpenPositions.Count.ShouldBe(1);
        state.Balance.ShouldBe(1000m - 990m);
        state.OpenPositions[symbolId].Volume.ShouldBe(9m);
    }

    [Fact]
    public void BuyCandidates_OrdersByScoreDescending_BuysHighestFirst()
    {
        // Arrange
        var symbolId1 = _fixture.Create<Id<Symbol>>();
        var symbolId2 = _fixture.Create<Id<Symbol>>();
        var symbol1 = Symbol.Create(
            symbolId1,
            "LOW",
            null,
            "Low Scorer",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var symbol2 = Symbol.Create(
            symbolId2,
            "HIGH",
            null,
            "High Scorer",
            _fixture.Create<Id<Market>>(),
            new AdditionalFields()
        );
        var scoredSymbols = new List<(Symbol Symbol, decimal Score, decimal Price)>
        {
            (symbol1, 0.2m, 100m), (symbol2, 0.9m, 100m)
        };
        var config = new StrategyConfiguration { BuyThreshold = 0.1m, MaxPositions = 1, MaxPositionPercent = 1m };
        var state = new BacktestLoopState(1000m);

        // Act
        BacktestEngine.BuyCandidates(scoredSymbols, config, 0m, state, DateTimeOffset.UtcNow, CancellationToken.None);

        // Assert
        state.OpenPositions.Count.ShouldBe(1);
        state.OpenPositions.ShouldContainKey(symbolId2);
    }

    [Fact]
    public void UpdatePortfolioMetrics_WhenBalanceOnly_TracksPortfolioValue()
    {
        // Arrange
        var state = new BacktestLoopState(10000m);
        var currentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var data = BacktestData.FromRaw(market, [], [], [], [], [], []);

        // Act
        BacktestEngine.UpdatePortfolioMetrics(state, currentDate, data);

        // Assert
        state.PortfolioValues.Count.ShouldBe(2);
        state.PortfolioValues[1].ShouldBe(10000m);
        state.PeakPortfolioValue.ShouldBe(10000m);
    }

    [Fact]
    public void UpdatePortfolioMetrics_WhenBalanceIncreases_UpdatesPeak()
    {
        // Arrange
        var state = new BacktestLoopState(10000m) { Balance = 12000m };
        var currentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var data = BacktestData.FromRaw(market, [], [], [], [], [], []);

        // Act
        BacktestEngine.UpdatePortfolioMetrics(state, currentDate, data);

        // Assert
        state.PeakPortfolioValue.ShouldBe(12000m);
        state.MaxDrawdown.ShouldBe(0m);
    }

    [Fact]
    public void UpdatePortfolioMetrics_WhenBalanceDecreases_TracksMaxDrawdown()
    {
        // Arrange
        var state = new BacktestLoopState(10000m) { PeakPortfolioValue = 10000m, Balance = 8000m };
        var currentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var data = BacktestData.FromRaw(market, [], [], [], [], [], []);

        // Act
        BacktestEngine.UpdatePortfolioMetrics(state, currentDate, data);

        // Assert
        state.MaxDrawdown.ShouldBe(0.2m);
    }

    [Fact]
    public void UpdatePortfolioMetrics_WithOpenPositions_IncludesPositionValue()
    {
        // Arrange
        var state = new BacktestLoopState(10000m) { Balance = 5000m };
        var symbolId = _fixture.Create<Id<Symbol>>();
        var currentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        state.OpenPositions[symbolId] = new OpenPosition(symbolId, "SYM", null, 100m, 50m, DateTimeOffset.UtcNow);

        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var dailyPrices = new List<DailyPrice> { new DailyPrice(symbolId, new DateOnly(2025, 1, 15), 100m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyPrices, []);

        // Act
        BacktestEngine.UpdatePortfolioMetrics(state, currentDate, data);

        // Assert
        state.PortfolioValues[1].ShouldBe(10000m);
    }

    [Fact]
    public void CreateClosedPosition_WhenProfitable_CalculatesPositiveReturn()
    {
        // Arrange
        var pos = new OpenPosition(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: DateTimeOffset.UtcNow.AddDays(-5)
        );
        var exitTime = DateTimeOffset.UtcNow;

        // Act
        var result = BacktestEngine.CreateClosedPosition(pos, 150m, 10m, 500m, exitTime);

        // Assert
        result.SymbolId.ShouldBe(pos.SymbolId.ToString());
        result.EntryPrice.ShouldBe(100m);
        result.ExitPrice.ShouldBe(150m);
        result.Volume.ShouldBe(10m);
        result.ProfitLoss.ShouldBe(500m);
        result.ReturnPercent.ShouldBe(0.5m);
        result.ExitTime.ShouldBe(exitTime);
    }

    [Fact]
    public void CreateClosedPosition_WhenLoss_CalculatesNegativeReturn()
    {
        // Arrange
        var pos = new OpenPosition(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            EntryPrice: 100m,
            Volume: 10m,
            EntryTime: DateTimeOffset.UtcNow.AddDays(-5)
        );
        var exitTime = DateTimeOffset.UtcNow;

        // Act
        var result = BacktestEngine.CreateClosedPosition(pos, 80m, 10m, -200m, exitTime);

        // Assert
        result.ProfitLoss.ShouldBe(-200m);
        result.ReturnPercent.ShouldBe(-0.2m);
    }

    [Fact]
    public void CreateClosedPosition_WhenZeroEntryPrice_ReturnsZeroReturn()
    {
        // Arrange
        var pos = new OpenPosition(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            EntryPrice: 0m,
            Volume: 10m,
            EntryTime: DateTimeOffset.UtcNow
        );
        var exitTime = DateTimeOffset.UtcNow;

        // Act
        var result = BacktestEngine.CreateClosedPosition(pos, 100m, 10m, 500m, exitTime);

        // Assert
        result.ReturnPercent.ShouldBe(0m);
    }

    [Fact]
    public void ComputeResults_WhenAllWinners_CalculatesMetrics()
    {
        // Arrange
        const decimal budget = 10000m;
        var state = new BacktestLoopState(budget) { Balance = 12000m, MaxDrawdown = 0.1m };
        state.PortfolioValues.AddRange([10500m, 11000m, 11500m, 12000m]);

        var pos1 = new BacktestPosition(
            "s1",
            "SYM1",
            100m,
            150m,
            10m,
            500m,
            0.5m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );
        var pos2 = new BacktestPosition(
            "s2",
            "SYM2",
            50m,
            75m,
            20m,
            500m,
            0.5m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );
        state.ClosedPositions.AddRange(pos1, pos2);

        // Act
        var results = BacktestEngine.ComputeResults(budget, state);

        // Assert
        results.TotalReturn.ShouldBe(2000m);
        results.TotalReturnPercent.ShouldBe(0.2m);
        results.WinningTrades.ShouldBe(2);
        results.LosingTrades.ShouldBe(0);
        results.TotalTrades.ShouldBe(2);
        results.WinRate.ShouldBe(1m);
        results.MaxDrawdown.ShouldBe(1000m);
        results.MaxDrawdownPercent.ShouldBe(0.1m);
        results.FinalBalance.ShouldBe(12000m);
    }

    [Fact]
    public void ComputeResults_WhenMixedTrades_CalculatesCorrectWinRate()
    {
        // Arrange
        const decimal budget = 10000m;
        var state = new BacktestLoopState(budget) { Balance = 10500m };
        state.PortfolioValues.AddRange([9500m, 10000m, 10500m]);

        var winPos = new BacktestPosition(
            "s1",
            "WIN",
            100m,
            150m,
            10m,
            500m,
            0.5m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );
        var lossPos = new BacktestPosition(
            "s2",
            "LOSS",
            100m,
            80m,
            10m,
            -200m,
            -0.2m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );
        state.ClosedPositions.AddRange(winPos, lossPos);

        // Act
        var results = BacktestEngine.ComputeResults(budget, state);

        // Assert
        results.TotalReturn.ShouldBe(500m);
        results.WinningTrades.ShouldBe(1);
        results.LosingTrades.ShouldBe(1);
        results.WinRate.ShouldBe(0.5m);
        results.AverageTradeReturn.ShouldBe(150m);
    }

    [Fact]
    public void ComputeResults_WhenNoTrades_ReturnsZeroMetrics()
    {
        // Arrange
        const decimal budget = 10000m;
        var state = new BacktestLoopState(budget) { Balance = 10000m };

        // Act
        var results = BacktestEngine.ComputeResults(budget, state);

        // Assert
        results.TotalReturn.ShouldBe(0m);
        results.TotalReturnPercent.ShouldBe(0m);
        results.WinRate.ShouldBe(0m);
        results.TotalTrades.ShouldBe(0);
        results.WinningTrades.ShouldBe(0);
        results.LosingTrades.ShouldBe(0);
        results.BestTrade.ShouldBe(0m);
        results.WorstTrade.ShouldBe(0m);
        results.AverageTradeReturn.ShouldBe(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenFewValues_ReturnsZero()
    {
        // Arrange
        var values = new List<decimal> { 10000m };

        // Act
        var result = BacktestEngine.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenConstantValues_ReturnsZero()
    {
        // Arrange
        var values = new List<decimal> { 10000m, 10000m, 10000m };

        // Act
        var result = BacktestEngine.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenIncreasingValues_ReturnsPositive()
    {
        // Arrange
        var values = new List<decimal>
        {
            10000m,
            10100m,
            10200m,
            10300m,
            10400m
        };

        // Act
        var result = BacktestEngine.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenDecreasingValues_ReturnsNegative()
    {
        // Arrange
        var values = new List<decimal>
        {
            10400m,
            10300m,
            10200m,
            10100m,
            10000m
        };

        // Act
        var result = BacktestEngine.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBeLessThan(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenTwoValues_ReturnsZero()
    {
        // Arrange
        var values = new List<decimal> { 100m, 200m };

        // Act
        var result = BacktestEngine.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenAllZeroValues_ReturnsZero()
    {
        // Arrange
        var values = new List<decimal>
        {
            0m,
            0m,
            0m,
            0m,
            0m
        };

        // Act
        var result = BacktestEngine.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void Constructor_WhenNullLogger_Throws()
    {
        // Arrange
        var dataService = CreateDataService();
        var composite = new CompositeExecutor([]);

        // Act
        var act = () => new BacktestEngine(null!, dataService, [], composite, []);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullDataService_Throws()
    {
        // Arrange
        var logger = Substitute.For<ILogger<BacktestEngine>>();
        var composite = new CompositeExecutor([]);

        // Act
        var act = () => new BacktestEngine(logger, null!, [], composite, []);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullCompositeExecutor_Throws()
    {
        // Arrange
        var logger = Substitute.For<ILogger<BacktestEngine>>();
        var dataService = CreateDataService();

        // Act
        var act = () => new BacktestEngine(logger, dataService, [], null!, []);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task RunAsync_WhenNoTrades_ReturnsResultsWithNoPositions()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration { BuyThreshold = 0.1m }
        );
        var market = Market.Create(marketId, "Test Market", new Taxes(null));

        var data = BacktestData.FromRaw(market, [symbol], [], [], [], [], []);

        var engine = CreateEngine();

        // Act
        var result = await engine.RunAsync(
            strategy,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m,
            CancellationToken.None,
            data: data
        );

        // Assert
        result.ShouldNotBeNull();
        result.TotalTrades.ShouldBe(0);
        result.Positions.ShouldBeEmpty();
        result.FinalBalance.ShouldBe(10000m);
    }

    [Fact]
    public async Task RunAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        var market = Market.Create(marketId, "Test Market", new Taxes(null));

        var data = BacktestData.FromRaw(market, [symbol], [], [], [], [], []);

        var engine = CreateEngine();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await engine.RunAsync(
            strategy,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m,
            cts.Token,
            data: data
        );

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RunAsync_WithTradesAndBuyThresholdZero_BuysPositions()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(symbolId, "SYM", null, "Test Symbol", marketId, new AdditionalFields());
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration { BuyThreshold = 0m, MaxPositions = 10 }
        );

        var baseTime = DateTimeOffset.UtcNow;
        var market = Market.Create(marketId, "Test Market", new Taxes(null));

        // Create daily aggregates and daily prices spanning the test window
        var aggregates = Enumerable.Range(0, 5)
            .Select(i => new DailyTradeAggregate(
                    symbolId,
                    DateOnly.FromDateTime(baseTime.AddDays(-3 + i).UtcDateTime),
                    100m,
                    100m,
                    100m,
                    10m
                )
            )
            .ToList();

        var dailyPrices = Enumerable.Range(0, 5)
            .Select(i => new DailyPrice(
                    symbolId,
                    DateOnly.FromDateTime(baseTime.AddDays(-3 + i).UtcDateTime),
                    100m
                )
            )
            .ToList();

        // Provide a signal so SignalWeightedExecutor produces a positive score
        var signal = Signal.Create(marketId, symbolId, SignalType.TaxAdjustedRoi, 0.8m);

        var data = BacktestData.FromRaw(
            market,
            [symbol],
            [],
            [],
            [signal],
            dailyPrices,
            aggregates
        );

        var engine = CreateEngine();

        var startDate = baseTime.AddDays(-3);
        var endDate = baseTime.AddDays(-1);

        // Act
        var result = await engine.RunAsync(
            strategy,
            marketId,
            startDate,
            endDate,
            10000m,
            CancellationToken.None,
            data: data
        );

        // Assert
        result.ShouldNotBeNull();
        result.FinalBalance.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task RunAsync_WithUnknownStrategyType_ThrowsInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.RecipeArbitrage,
            new StrategyConfiguration { MinMarginPercent = 0.01m }
        );
        var market = Market.Create(marketId, "Test Market", new Taxes(null));

        var data = BacktestData.FromRaw(market, [symbol], [], [], [], [], []);

        // No executor registered for RecipeArbitrage
        var engine = CreateEngine(executors: []);

        // Act
        var act = async () => await engine.RunAsync(
            strategy,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow,
            10000m,
            CancellationToken.None,
            data: data
        );

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    private static BacktestEngine CreateEngine(
        List<IStrategyExecutor>? executors = null,
        List<ISignalComputer>? signalComputers = null
    )
    {
        var logger = Substitute.For<ILogger<BacktestEngine>>();
        var dataService = CreateDataService();
        executors ??= [new SignalWeightedExecutor()];
        var composite = new CompositeExecutor(executors);
        signalComputers ??= [];

        return new BacktestEngine(logger, dataService, executors, composite, signalComputers);
    }

    private static BacktestDataQueryService CreateDataService()
    {
        var logger = Substitute.For<ILogger<BacktestDataQueryService>>();
        var factory = DbContextExtensions.MockFactory<PlutusDbContext>();
        return new BacktestDataQueryService(logger, factory);
    }
}
