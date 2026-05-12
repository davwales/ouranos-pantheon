using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class BacktestMathTests
{
    private readonly IFixture _fixture = new Fixture();

    public BacktestMathTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void ComputeExit_WhenVolumeExceedsDailyParticipation_CapsSellVolume()
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
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));

        // Act
        var (_, exitVolume, _) = BacktestMath.ComputeExit(pos, 150m, 0m, market, 100m, 0.25m, 0m);

        // Assert
        exitVolume.ShouldBe(25m);
    }

    [Fact]
    public void ComputeExit_WhenSlippageApplied_ReducesExitPrice()
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
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));

        // Act
        var (netProceeds, exitVolume, _) = BacktestMath.ComputeExit(
            pos,
            100m,
            0m,
            market,
            100m,
            0.25m,
            0.1m
        );

        // Assert
        exitVolume.ShouldBe(25m);
        netProceeds.ShouldBe(2437.5m);
    }

    [Fact]
    public void ComputeExit_WhenParticipationRateZero_ReturnsZero()
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
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));

        // Act
        var (netProceeds, exitVolume, netPnl) = BacktestMath.ComputeExit(
            pos,
            150m,
            0m,
            market,
            100m,
            0m,
            0m
        );

        // Assert
        exitVolume.ShouldBe(0m);
        netProceeds.ShouldBe(0m);
        netPnl.ShouldBe(0m);
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
        var (netProceeds, exitVolume, netPnl) = BacktestMath.ComputeExit(
            pos,
            150m,
            0.05m,
            market,
            0m,
            0.25m,
            0m
        );

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
        var (netProceeds, _, netPnl) = BacktestMath.ComputeExit(
            pos,
            200m,
            0.10m,
            market,
            0m,
            0.25m,
            0m
        );

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
        var (_, _, netPnl) = BacktestMath.ComputeExit(pos, 50m, 0.05m, market, 0m, 0.25m, 0m);

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
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));

        // Act
        var (netProceeds, _, netPnl) = BacktestMath.ComputeExit(
            pos,
            60m,
            0m,
            market,
            0m,
            0.25m,
            0m
        );

        // Assert
        netProceeds.ShouldBe(1200m);
        netPnl.ShouldBe(200m);
    }

    [Fact]
    public void GetForecastData_WhenForecastIsNull_ReturnsNulls()
    {
        // Arrange
        const decimal currentPrice = 100m;

        // Act
        var (price, change) = BacktestMath.GetForecastData(null, currentPrice);

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
        var (price, change) = BacktestMath.GetForecastData(forecast, 0m);

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
        var (price, change) = BacktestMath.GetForecastData(forecast, 100m);

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
        var (price, change) = BacktestMath.GetForecastData(forecast, currentPrice);

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
        var (price, change) = BacktestMath.GetForecastData(forecast, currentPrice);

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
        var result = BacktestMath.BuildPriceBucketsFromAggregates(aggregates);

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
            new(symbolId, new DateOnly(2025, 1, 15), 100m, 100m, 100m, 50m),
        };

        // Act
        var result = BacktestMath.BuildPriceBucketsFromAggregates(aggregates);

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
        var aggregates = Enumerable
            .Range(0, 50)
            .Select(i => new DailyTradeAggregate(
                symbolId,
                new DateOnly(2025, 1, 1).AddDays(i),
                100m + i,
                100m + i,
                100m + i,
                10m + i
            ))
            .ToList();

        // Act
        var result = BacktestMath.BuildPriceBucketsFromAggregates(aggregates);

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
        var aggregates = Enumerable
            .Range(0, 10)
            .Select(i => new DailyTradeAggregate(
                symbolId,
                new DateOnly(2025, 1, 1).AddDays(i),
                100m + i,
                100m + i,
                100m + i,
                10m
            ))
            .ToList();

        // Act
        var result = BacktestMath.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.Count.ShouldBe(10);
    }

    [Fact]
    public void BuildPriceBucketsFromAggregates_WhenExactly25Aggregates_CreatesExactly25Buckets()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var aggregates = Enumerable
            .Range(0, 25)
            .Select(i => new DailyTradeAggregate(
                symbolId,
                new DateOnly(2025, 1, 1).AddDays(i),
                100m + i,
                100m + i,
                100m + i,
                10m
            ))
            .ToList();

        // Act
        var result = BacktestMath.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.Count.ShouldBe(25);
        foreach (var bucket in result)
        {
            bucket.MinPrice.ShouldBeLessThanOrEqualTo(bucket.MaxPrice);
        }
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
        var result = BacktestMath.CreateClosedPosition(pos, 150m, 10m, 500m, exitTime);

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
        var result = BacktestMath.CreateClosedPosition(pos, 80m, 10m, -200m, exitTime);

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
        var result = BacktestMath.CreateClosedPosition(pos, 100m, 10m, 500m, exitTime);

        // Assert
        result.ReturnPercent.ShouldBe(0m);
    }
}
