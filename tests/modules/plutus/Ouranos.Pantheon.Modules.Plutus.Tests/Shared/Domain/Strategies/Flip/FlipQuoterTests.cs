using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Flip;

public sealed class FlipQuoterTests
{
    private static readonly FlipConfiguration Configuration = new(
        LookbackHours: 4,
        MinMargin: 0.01m,
        MaxPriceMove: 0m
    );

    [Fact]
    public void Quote_ShouldBidFlooredLowAndAskCeiledHigh()
    {
        // Arrange
        var series = FlipTestData.Bouncing(10, 100.4f, 110.2f);

        // Act
        var quote = FlipQuoter.Quote(series, 8, Configuration, FlipTestData.OsrsRules);

        // Assert
        quote.ShouldNotBeNull();
        quote.Bid.ShouldBe(100m);
        quote.Ask.ShouldBe(111m);
        quote.NetMargin.ShouldBe((111m - 2m - 100m) / 100m);
    }

    [Fact]
    public void Quote_ShouldMoveBidAndAskInsideTheSpreadByTheOffsets()
    {
        // Arrange
        var series = FlipTestData.Bouncing(10, 100f, 200f);
        var configuration = Configuration with { BidOffset = 0.1m, AskOffset = 0.2m };

        // Act
        var quote = FlipQuoter.Quote(series, 8, configuration, FlipTestData.OsrsRules);

        // Assert
        quote.ShouldNotBeNull();
        quote.Bid.ShouldBe(110m);
        quote.Ask.ShouldBe(180m);
    }

    [Fact]
    public void Quote_WhenTooFewBucketsTradedOnBothSides_ShouldReturnNull()
    {
        // Arrange
        var series = FlipTestData.Series(
            10,
            i => i % 2 == 0 ? new FlipBar(110f, 10f, 100f, 10f) : default
        );

        // Act
        var quote = FlipQuoter.Quote(series, 8, Configuration, FlipTestData.OsrsRules);

        // Assert
        quote.ShouldBeNull();
    }

    [Fact]
    public void Quote_WhenLookbackStartsBeforeTheData_ShouldReturnNull()
    {
        // Arrange
        var series = FlipTestData.Bouncing(10, 100f, 110f);

        // Act
        var quote = FlipQuoter.Quote(series, 3, Configuration, FlipTestData.OsrsRules);

        // Assert
        quote.ShouldBeNull();
    }

    [Fact]
    public void Quote_WhenBidIsBelowMinPrice_ShouldReturnNull()
    {
        // Arrange
        var series = FlipTestData.Bouncing(10, 40f, 45f);

        // Act
        var quote = FlipQuoter.Quote(
            series,
            8,
            Configuration with
            {
                MinPrice = 50m,
            },
            FlipTestData.OsrsRules
        );

        // Assert
        quote.ShouldBeNull();
    }

    [Fact]
    public void Quote_WhenPriceMovedTooFarFromItsWeeklyLevel_ShouldReturnNull()
    {
        // Arrange
        var hours = (FlipQuoter.PriceMoveDays * 24) + 10;

        var series = FlipTestData.Series(
            hours,
            i =>
                i < (hours - 6) * FlipMarketData.BarsPerHour
                    ? new FlipBar(110f, 10f, 100f, 10f)
                    : new FlipBar(165f, 10f, 150f, 10f)
        );

        // Act
        var quote = FlipQuoter.Quote(
            series,
            hours,
            Configuration with
            {
                MaxPriceMove = 0.1m,
            },
            FlipTestData.OsrsRules
        );

        // Assert
        quote.ShouldBeNull();
    }

    [Fact]
    public void ExpectedUnits_ShouldOnlyCountBucketsWhereTheOfferWouldHaveFilled()
    {
        // Arrange
        var series = FlipTestData.Series(
            10,
            i =>
                i % 2 == 0
                    ? new FlipBar(110f, 100f, 100f, 100f)
                    : new FlipBar(105f, 100f, 102f, 100f)
        );

        var quote = new FlipQuote(series, 100m, 110m, 101m, 0.08m);
        var configuration = Configuration with { CheckInHours = 4 };

        // Act
        var units = FlipQuoter.ExpectedUnits(quote, 8, configuration, 0.1m);

        // Assert
        units.ShouldBe(0.1m * 100m * 4 * FlipMarketData.BarsPerHour / 2);
    }

    [Fact]
    public void ExpectedUnits_ShouldBeCappedByTheBuyLimitPerCheckIn()
    {
        // Arrange
        var series = FlipTestData.Bouncing(10, 100f, 110f, volume: 1_000_000f, buyLimit: 50m);
        var quote = new FlipQuote(series, 100m, 110m, 100m, 0.08m);

        // Act
        var units = FlipQuoter.ExpectedUnits(
            quote,
            8,
            Configuration with
            {
                CheckInHours = 8,
            },
            0.1m
        );

        // Assert
        units.ShouldBe(100m);
    }

    [Fact]
    public void Rank_ShouldDropThinMarginsAndOrderByExpectedProfit()
    {
        // Arrange
        var thin = FlipTestData.Bouncing(10, 100f, 101f, name: "Thin");
        var small = FlipTestData.Bouncing(10, 100f, 120f, volume: 10f, name: "Small");
        var large = FlipTestData.Bouncing(10, 100f, 120f, volume: 1000f, name: "Large");

        // Act
        var ranked = FlipQuoter.Rank(
            [thin, small, large],
            8,
            Configuration,
            FlipTestData.OsrsRules,
            0.1m
        );

        // Assert
        ranked.Select(c => c.Quote.Series.Name).ShouldBe(["Large", "Small"]);
        ranked[0]
            .ExpectedProfit.ShouldBe(ranked[0].Quote.NetMargin * 100m * ranked[0].ExpectedUnits);
    }
}
