using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Swing;

public sealed class SwingMathTests
{
    private readonly Fixture _fixture = new();

    public SwingMathTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void ComputeExit_WhenVolumeExceedsFillCap_ShouldCapSellVolume()
    {
        // Act
        var (netProceeds, exitVolume) = SwingMath.ComputeExit(
            100m,
            150m,
            MarketRules.TaxFree,
            100m,
            25m,
            0m
        );

        // Assert
        exitVolume.ShouldBe(25m);
        netProceeds.ShouldBe(3750m);
    }

    [Fact]
    public void ComputeExit_WhenSlippageApplied_ShouldReduceExitPrice()
    {
        // Act
        var (netProceeds, exitVolume) = SwingMath.ComputeExit(
            100m,
            100m,
            MarketRules.TaxFree,
            100m,
            25m,
            0.1m
        );

        // Assert
        exitVolume.ShouldBe(25m);
        netProceeds.ShouldBe(2375m);
    }

    [Theory]
    [InlineData(150, 0)]
    [InlineData(0, 25)]
    public void ComputeExit_WhenFillCapOrPriceIsZero_ShouldSellNothing(
        decimal exitPrice,
        decimal fillCap
    )
    {
        // Act
        var (netProceeds, exitVolume) = SwingMath.ComputeExit(
            100m,
            exitPrice,
            MarketRules.TaxFree,
            100m,
            fillCap,
            0m
        );

        // Assert
        exitVolume.ShouldBe(0m);
        netProceeds.ShouldBe(0m);
    }

    [Fact]
    public void ComputeExit_WhenMarketIsTaxed_ShouldDeductRoundedDownUnitTax()
    {
        // Arrange
        var rules = new MarketRules(new FlatTax(0m, 1000m, 0.05m));

        // Act
        var (netProceeds, exitVolume) = SwingMath.ComputeExit(10m, 150m, rules, 0m, 1000m, 0m);

        // Assert
        exitVolume.ShouldBe(10m);
        netProceeds.ShouldBe(1430m);
    }

    [Fact]
    public void ComputeExit_WhenUnitTaxExceedsMaximum_ShouldCapTaxPerUnit()
    {
        // Arrange
        var rules = new MarketRules(new FlatTax(0m, 5m, 0.10m));

        // Act
        var (netProceeds, _) = SwingMath.ComputeExit(100m, 200m, rules, 0m, 1000m, 0m);

        // Assert
        netProceeds.ShouldBe(19500m);
    }

    [Fact]
    public void ComputeExit_WhenNoTax_ShouldReturnFullGross()
    {
        // Act
        var (netProceeds, exitVolume) = SwingMath.ComputeExit(
            20m,
            60m,
            MarketRules.TaxFree,
            0m,
            1000m,
            0m
        );

        // Assert
        exitVolume.ShouldBe(20m);
        netProceeds.ShouldBe(1200m);
    }

    [Fact]
    public void ComputeSlippage_WhenVolumeIsLargeRelativeToDay_ShouldCapAtMaxSlippage()
    {
        // Arrange
        const decimal volume = 10_000m;
        const decimal dayVolume = 1m;

        // Act
        var slippage = SwingMath.ComputeSlippage(volume, dayVolume, 0.1m);

        // Assert
        slippage.ShouldBe(SwingMath.MaxSlippage);
    }

    [Fact]
    public void ComputeSlippage_WhenDayVolumeZero_ShouldReturnZero()
    {
        // Act
        var slippage = SwingMath.ComputeSlippage(10m, 0m, 0.1m);

        // Assert
        slippage.ShouldBe(0m);
    }

    [Fact]
    public void BuildPriceBucketsFromAggregates_WhenEmptyAggregates_ReturnsEmptyList()
    {
        // Arrange
        var aggregates = new List<DailyTradeAggregate>();

        // Act
        var result = SwingMath.BuildPriceBucketsFromAggregates(aggregates);

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
        var result = SwingMath.BuildPriceBucketsFromAggregates(aggregates);

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
        var result = SwingMath.BuildPriceBucketsFromAggregates(aggregates);

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
        var result = SwingMath.BuildPriceBucketsFromAggregates(aggregates);

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
        var result = SwingMath.BuildPriceBucketsFromAggregates(aggregates);

        // Assert
        result.Count.ShouldBe(25);
        foreach (var bucket in result)
        {
            bucket.MinPrice.ShouldBeLessThanOrEqualTo(bucket.MaxPrice);
        }
    }
}
