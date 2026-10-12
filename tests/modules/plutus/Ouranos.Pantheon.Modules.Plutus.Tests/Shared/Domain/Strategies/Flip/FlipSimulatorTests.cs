using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Flip;

public sealed class FlipSimulatorTests
{
    private const int WarmUpHours = 4;

    private static readonly FlipConfiguration Configuration = new(
        CheckInHours: 8,
        MaxOpenOrders: 8,
        LookbackHours: WarmUpHours,
        MinMargin: 0.01m,
        MaxHoldHours: 1000,
        MaxPositionPercent: 0.5m,
        MaxPriceMove: 0m
    );

    private static SimulationResult Run(
        FlipMarketData data,
        int hours,
        FlipConfiguration? configuration = null,
        decimal budget = 1_000_000m
    )
    {
        var start = FlipTestData.Start.AddHours(WarmUpHours);
        return FlipSimulator.Run(
            data,
            configuration ?? Configuration,
            new SimulationSettings(start, start.AddHours(hours), budget, 0.1m, 0m)
        );
    }

    private static Func<int, FlipBar> AfterWarmUp(FlipBar warmUp, FlipBar after)
    {
        return i => i < WarmUpHours * FlipMarketData.BarsPerHour ? warmUp : after;
    }

    [Fact]
    public void Run_WhenAnItemBouncesBetweenStablePrices_ShouldProfitFromEveryRoundTrip()
    {
        // Arrange
        var data = FlipTestData.Market(FlipTestData.Bouncing(60, 100f, 120f));

        // Act
        var result = Run(data, 48);

        // Assert
        var end = FlipTestData.Start.AddHours(WarmUpHours + 48);
        var roundTrips = result.Positions.Where(p => p.ExitTime < end.AddMinutes(-5)).ToList();
        roundTrips.ShouldNotBeEmpty();
        roundTrips.ShouldAllBe(p => p.EntryPrice == 100m && p.ExitPrice == 120m);
        roundTrips.ShouldAllBe(p => p.ProfitLoss == p.Volume * (120m - 2m - 100m));
        result.FinalBalance.ShouldBe(1_000_000m + result.Positions.Sum(p => p.ProfitLoss));
    }

    [Fact]
    public void Run_ShouldNotSellAnythingBeforeTheNextCheckIn()
    {
        // Arrange
        var data = FlipTestData.Market(FlipTestData.Bouncing(60, 100f, 120f));

        // Act
        var result = Run(data, 48);

        // Assert
        var firstCheckIn = FlipTestData.Start.AddHours(WarmUpHours);
        result.Positions.ShouldAllBe(p =>
            p.ExitTime >= firstCheckIn.AddHours(Configuration.CheckInHours)
        );
    }

    [Fact]
    public void Run_ShouldRespectTheBuyLimitPerFourHours()
    {
        // Arrange
        var data = FlipTestData.Market(
            FlipTestData.Bouncing(60, 100f, 120f, volume: 1_000_000f, buyLimit: 10m)
        );

        // Act
        var result = Run(data, 48);

        // Assert
        result.Positions.ShouldNotBeEmpty();
        result.Positions.ShouldAllBe(p => p.Volume <= 20m);
    }

    [Fact]
    public void Run_WhenInventoryUsesEveryOpenOrder_ShouldNotPlaceNewBuyOrders()
    {
        // Arrange
        var better = FlipTestData.Bouncing(60, 100f, 130f, name: "Better");
        var worse = FlipTestData.Bouncing(60, 100f, 110f, name: "Worse");
        var data = FlipTestData.Market(better, worse);

        // Act
        var result = Run(data, 48, Configuration with { MaxOpenOrders = 1 });

        // Assert
        result.Positions.ShouldNotBeEmpty();
        result.Positions.ShouldAllBe(p => p.SymbolName == "Better");
    }

    [Fact]
    public void Run_WhenInventoryIsHeldPastMaxHold_ShouldDumpItAtTheInstaSellPrice()
    {
        // Arrange
        var series = FlipTestData.Series(
            60,
            AfterWarmUp(new FlipBar(120f, 1000f, 100f, 1000f), new FlipBar(90f, 1000f, 80f, 1000f))
        );

        // Act
        var result = Run(
            FlipTestData.Market(series),
            48,
            Configuration with
            {
                MaxOpenOrders = 1,
                MaxHoldHours = 12,
            }
        );

        // Assert
        var dumped = result.Positions[0];
        dumped.EntryPrice.ShouldBe(100m);
        dumped.ExitPrice.ShouldBe(80m);
        dumped.ProfitLoss.ShouldBeLessThan(0m);
    }

    [Fact]
    public void Run_WhenNoItemClearsTheMinimumMargin_ShouldNotTrade()
    {
        // Arrange
        var series = FlipTestData.Series(
            30,
            AfterWarmUp(new FlipBar(120f, 1000f, 100f, 1000f), new FlipBar(101f, 1000f, 99f, 1000f))
        );

        // Act
        var result = Run(FlipTestData.Market(series), 20, Configuration with { MinMargin = 0.5m });

        // Assert
        result.Positions.ShouldBeEmpty();
        result.FinalBalance.ShouldBe(1_000_000m);
    }

    [Fact]
    public void Run_WhenInventoryIsUnsoldAtTheEnd_ShouldMarkItAtTheLastInstaSellPriceAfterTax()
    {
        // Arrange
        var series = FlipTestData.Series(
            30,
            AfterWarmUp(new FlipBar(120f, 1000f, 100f, 1000f), new FlipBar(101f, 1000f, 99f, 1000f))
        );

        // Act
        var result = Run(FlipTestData.Market(series), 7);

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.EntryPrice.ShouldBe(100m);
        position.ExitPrice.ShouldBe(99m);
        position.ProfitLoss.ShouldBe(position.Volume * (99m - 1m - 100m));
    }

    [Fact]
    public void Run_ShouldRecordDailyEquityAndDrawdown()
    {
        // Arrange
        var data = FlipTestData.Market(FlipTestData.Bouncing(60, 100f, 120f));

        // Act
        var result = Run(data, 48);

        // Assert
        result.Equity.Values.Count.ShouldBe(3);
        result.Equity.Values[0].ShouldBe(1_000_000m);
        result.Equity.MaxDrawdown.ShouldBeGreaterThanOrEqualTo(0m);
        result.Equity.Peak.ShouldBe(result.Equity.Values.Max());
    }

    [Fact]
    public void PlanOrders_ShouldFillNoMoreThanTheFreeOrders()
    {
        // Arrange
        var candidates = Candidates(100m, 100m, 100m);

        // Act
        var orders = FlipSimulator.PlanOrders(candidates, 2, 1_000m, 1_000_000m, _ => 0m).ToList();

        // Assert
        orders.Select(o => o.Candidate).ShouldBe(candidates.Take(2));
        orders.ShouldAllBe(o => o.Quantity == 10m);
    }

    [Fact]
    public void PlanOrders_ShouldCapEachOfferAtTheExpectedUnits()
    {
        // Arrange
        var candidates = Candidates(4.7m);

        // Act
        var (Candidate, Quantity) = FlipSimulator
            .PlanOrders(candidates, 8, 1_000m, 1_000_000m, _ => 0m)
            .ShouldHaveSingleItem();

        // Assert
        Quantity.ShouldBe(4m);
    }

    [Fact]
    public void PlanOrders_ShouldDeductTheCostOfInventoryAlreadyHeldFromTheItemBudget()
    {
        // Arrange
        var candidates = Candidates(100m, 100m);
        var held = candidates[0].Quote.Series;

        // Act
        var orders = FlipSimulator
            .PlanOrders(candidates, 8, 1_000m, 1_000_000m, s => s == held ? 650m : 0m)
            .ToList();

        // Assert
        orders.Select(o => o.Quantity).ShouldBe([3m, 10m]);
    }

    [Fact]
    public void PlanOrders_WhenHeldCostExceedsTheItemBudget_ShouldSkipWithoutUsingASlot()
    {
        // Arrange
        var candidates = Candidates(100m, 100m);
        var held = candidates[0].Quote.Series;

        // Act
        var orders = FlipSimulator
            .PlanOrders(candidates, 1, 1_000m, 1_000_000m, s => s == held ? 2_000m : 0m)
            .ToList();

        // Assert
        orders.ShouldHaveSingleItem().Candidate.ShouldBe(candidates[1]);
    }

    [Fact]
    public void PlanOrders_ShouldNeverSpendMoreThanTheCashLeft()
    {
        // Arrange
        var candidates = Candidates(100m, 100m, 100m);

        // Act
        var orders = FlipSimulator.PlanOrders(candidates, 8, 1_000m, 1_500m, _ => 0m).ToList();

        // Assert
        orders.Select(o => o.Quantity).ShouldBe([10m, 5m]);
        orders.Sum(o => o.Quantity * o.Candidate.Quote.Bid).ShouldBeLessThanOrEqualTo(1_500m);
    }

    private static List<FlipCandidate> Candidates(params decimal[] expectedUnits)
    {
        return
        [
            .. expectedUnits.Select(
                (units, i) =>
                    new FlipCandidate(
                        new FlipQuote(
                            FlipTestData.Bouncing(1, 100f, 120f, name: $"Item{i}"),
                            100m,
                            120m,
                            100m,
                            0.18m
                        ),
                        units,
                        units * 18m
                    )
            ),
        ];
    }
}
