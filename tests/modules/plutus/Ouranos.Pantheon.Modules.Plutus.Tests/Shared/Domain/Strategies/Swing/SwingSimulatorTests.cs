using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Swing;

public sealed class SwingSimulatorTests
{
    private static readonly BacktestDataOptions Options = new(
        LookbackDays: 0,
        LimitWindowsPerDay: 2m,
        MinFillFraction: 0m
    );

    private static readonly DateOnly Day0 = SwingTestData.Day(SwingTestData.Start);

    private readonly Market _market = SwingTestData.CreateMarket();

    private static SwingConfiguration Configuration(
        decimal? buyThreshold = null,
        decimal? sellThreshold = null,
        int? maxPositions = null,
        decimal? maxPositionPercent = null,
        int? holdPeriodDays = null
    )
    {
        return new SwingConfiguration(
            StrategyTestFactory.DefaultWeights(),
            buyThreshold,
            sellThreshold,
            maxPositions,
            maxPositionPercent,
            holdPeriodDays
        );
    }

    private static SimulationResult Run(
        SwingMarketData market,
        SwingSignalTable signals,
        SwingConfiguration configuration,
        int days,
        decimal budget = 1000m,
        decimal participationRate = 1m,
        decimal slippageMultiplier = 0m,
        BacktestDataOptions? options = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        return SwingSimulator.Run(
            new SwingData(market, signals),
            configuration,
            new SimulationSettings(
                SwingTestData.Start,
                SwingTestData.Start.AddDays(days),
                budget,
                participationRate,
                slippageMultiplier,
                progress,
                cancellationToken
            ),
            options ?? Options
        );
    }

    private SwingMarketData MarketData(
        IReadOnlyList<Symbol> symbols,
        IEnumerable<DailyTradeAggregate> aggregates
    )
    {
        return SwingMarketData.FromRaw(_market, [.. symbols], [.. aggregates]);
    }

    private static SwingSignalTable Constant(
        int days,
        IReadOnlyList<Symbol> symbols,
        Dictionary<string, decimal> scores
    )
    {
        return SwingTestData.Signals(days, symbols, (_, s) => SwingTestData.Signal(scores[s.Name]));
    }

    [Fact]
    public void Run_WhenSymbolsScoreAboveBuyThreshold_ShouldBuyBestFirstFromRemainingCash()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var b = SwingTestData.CreateSymbol(_market, "B");
        var c = SwingTestData.CreateSymbol(_market, "C");
        Symbol[] symbols = [c, b, a];

        var market = MarketData(
            symbols,
            symbols.SelectMany(s => SwingTestData.Steady(s, 2, 10m, 10_000m))
        );

        var signals = Constant(
            2,
            symbols,
            new Dictionary<string, decimal>
            {
                ["A"] = 0.9m,
                ["B"] = 0.5m,
                ["C"] = 0.1m,
            }
        );

        // Act
        var result = Run(
            market,
            signals,
            Configuration(buyThreshold: 0.3m, maxPositionPercent: 0.5m),
            2
        );

        // Assert
        result.Positions.Count.ShouldBe(2);
        var first = result.Positions.Single(p => p.SymbolName == "A");
        first.Volume.ShouldBe(50m);
        first.EntryTime.ShouldBe(SwingTestData.Start);
        var second = result.Positions.Single(p => p.SymbolName == "B");
        second.Volume.ShouldBe(25m);
        second.EntryTime.ShouldBe(SwingTestData.Start);
    }

    [Fact]
    public void Run_WhenMaxPositionsReached_ShouldNotBuyMore()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var b = SwingTestData.CreateSymbol(_market, "B");
        Symbol[] symbols = [a, b];

        var market = MarketData(
            symbols,
            symbols.SelectMany(s => SwingTestData.Steady(s, 3, 10m, 10_000m))
        );

        var signals = Constant(
            3,
            symbols,
            new Dictionary<string, decimal> { ["A"] = 0.9m, ["B"] = 0.5m }
        );

        // Act
        var result = Run(
            market,
            signals,
            Configuration(maxPositions: 1, maxPositionPercent: 0.5m),
            3
        );

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.SymbolName.ShouldBe("A");
        position.Volume.ShouldBe(50m);
    }

    [Fact]
    public void Run_WhenHoldPeriodElapses_ShouldSellOnThatDay()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 5, 10m, 10_000m));
        var signals = Constant(5, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(market, signals, Configuration(holdPeriodDays: 2), 5);

        // Assert
        result.Positions[0].EntryTime.ShouldBe(SwingTestData.Start);
        result.Positions[0].ExitTime.ShouldBe(SwingTestData.Start.AddDays(2));
        result.Positions.ShouldAllBe(p => (p.ExitTime - p.EntryTime).TotalDays <= 2);
    }

    [Fact]
    public void Run_WhenHeldScoreFallsBelowSellThreshold_ShouldSell()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 3, 10m, 10_000m));

        var signals = SwingTestData.Signals(
            3,
            [a],
            (day, _) => SwingTestData.Signal(day == 0 ? 0.9m : -0.5m)
        );

        // Act
        var result = Run(market, signals, Configuration(sellThreshold: 0m), 3);

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.ExitTime.ShouldBe(SwingTestData.Start.AddDays(1));
    }

    [Fact]
    public void Run_WhenNoSellThreshold_ShouldHoldUntilTheEnd()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 3, 10m, 10_000m));

        var signals = SwingTestData.Signals(
            3,
            [a],
            (day, _) => SwingTestData.Signal(day == 0 ? 0.9m : -0.5m)
        );

        // Act
        var result = Run(market, signals, Configuration(), 3);

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.ExitTime.ShouldBe(SwingTestData.Start.AddDays(3));
    }

    [Fact]
    public void Run_WhenSymbolsAreIneligible_ShouldNotBuyThemButStillSellHeldOnesOnScore()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var b = SwingTestData.CreateSymbol(_market, "B");
        Symbol[] symbols = [a, b];

        var market = MarketData(
            symbols,
            symbols.SelectMany(s => SwingTestData.Steady(s, 3, 10m, 10_000m))
        );

        var signals = SwingTestData.Signals(
            3,
            symbols,
            (day, s) =>
                s == b ? SwingTestData.Signal(0.9m, isEligible: false)
                : day == 0 ? SwingTestData.Signal(0.9m)
                : SwingTestData.Signal(-0.5m, isEligible: false)
        );

        // Act
        var result = Run(
            market,
            signals,
            Configuration(sellThreshold: 0m, maxPositionPercent: 0.5m),
            3
        );

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.SymbolName.ShouldBe("A");
        position.ExitTime.ShouldBe(SwingTestData.Start.AddDays(1));
    }

    [Fact]
    public void Run_WhenBuyLimitIsZero_ShouldSkipTheSymbol()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A", limit: 0m);
        var b = SwingTestData.CreateSymbol(_market, "B");
        Symbol[] symbols = [a, b];

        var market = MarketData(
            symbols,
            symbols.SelectMany(s => SwingTestData.Steady(s, 2, 10m, 10_000m))
        );

        var signals = Constant(
            2,
            symbols,
            new Dictionary<string, decimal> { ["A"] = 0.9m, ["B"] = 0.5m }
        );

        // Act
        var result = Run(market, signals, Configuration(), 2);

        // Assert
        result.Positions.ShouldHaveSingleItem().SymbolName.ShouldBe("B");
    }

    [Fact]
    public void Run_WhenSymbolHasBuyLimit_ShouldCapVolumeAtLimitWindowsPerDay()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A", limit: 15m);
        var market = MarketData([a], SwingTestData.Steady(a, 1, 10m, 10_000m));
        var signals = Constant(1, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(market, signals, Configuration(maxPositions: 1), 1);

        // Assert
        result.Positions.ShouldHaveSingleItem().Volume.ShouldBe(30m);
    }

    [Fact]
    public void Run_WhenVolumeIsThin_ShouldCapFillsAtParticipationRate()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 1, 10m, 100m));
        var signals = Constant(1, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(
            market,
            signals,
            Configuration(maxPositions: 1),
            1,
            participationRate: 0.1m
        );

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.Volume.ShouldBe(10m);
        result.FinalBalance.ShouldBe(1000m);
    }

    [Fact]
    public void Run_WhenTypicalVolumeIsZero_ShouldNotBuyOnASingleBusyDay()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, Day0, Day0.AddDays(1), 10m, 10_000m));
        var signals = Constant(1, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(market, signals, Configuration(), 1);

        // Assert
        result.Positions.ShouldBeEmpty();
        result.FinalBalance.ShouldBe(1000m);
    }

    [Fact]
    public void Run_WhenSymbolDidNotTradeThatDay_ShouldNotBuy()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");

        var market = MarketData(
            [a],
            SwingTestData.Steady(
                a,
                Day0.AddDays(-SwingMarketData.TypicalVolumeDays),
                Day0.AddDays(-1),
                10m,
                10_000m
            )
        );

        var signals = Constant(2, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(market, signals, Configuration(), 2);

        // Assert
        result.Positions.ShouldBeEmpty();
        result.FinalBalance.ShouldBe(1000m);
    }

    [Fact]
    public void Run_WhenCappedFillIsBelowMinFillFraction_ShouldSkipAndBuyTheNextCandidate()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var b = SwingTestData.CreateSymbol(_market, "B");

        var market = MarketData(
            [a, b],
            SwingTestData.Steady(a, 1, 10m, 50m).Concat(SwingTestData.Steady(b, 1, 10m, 10_000m))
        );

        var signals = Constant(
            1,
            [a, b],
            new Dictionary<string, decimal> { ["A"] = 0.9m, ["B"] = 0.5m }
        );

        // Act
        var result = Run(
            market,
            signals,
            Configuration(maxPositions: 1),
            1,
            participationRate: 0.1m,
            options: Options with
            {
                MinFillFraction = 0.1m,
            }
        );

        // Assert
        result.Positions.ShouldHaveSingleItem().SymbolName.ShouldBe("B");
    }

    [Fact]
    public void Run_WhenSlippageApplies_ShouldRaiseTheEntryPrice()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 1, 10m, 100m));
        var signals = Constant(1, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(
            market,
            signals,
            Configuration(),
            1,
            budget: 250m,
            slippageMultiplier: 0.1m
        );

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.EntryPrice.ShouldBe(10.5m);
        position.Volume.ShouldBe(23m);
    }

    [Fact]
    public void Run_WhenLiquidityCannotAbsorbTheEndSale_ShouldMarkTheResidualAtTheDayMinimumAfterTax()
    {
        // Arrange
        var market = SwingTestData.CreateMarket(SwingTestData.OsrsTax);
        var a = SwingTestData.CreateSymbol(market, "A");

        var aggregates = SwingTestData
            .Steady(a, 1, 1000m, 1000m)
            .Append(new DailyTradeAggregate(a.Id, Day0.AddDays(2), 1200m, 800m, 1200m, 40m));

        var data = SwingMarketData.FromRaw(market, [a], [.. aggregates]);
        var signals = Constant(2, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(data, signals, Configuration(), 2, budget: 100_000m);

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.Volume.ShouldBe(100m);
        position.EntryPrice.ShouldBe(1000m);
        position.ExitPrice.ShouldBe(960m);
        position.ExitTime.ShouldBe(SwingTestData.Start.AddDays(2));
        position.ProfitLoss.ShouldBe(-5920m);
        result.FinalBalance.ShouldBe(94_080m);
    }

    [Fact]
    public void Run_WhenHeldSymbolDoesNotTradeOnTheLastDay_ShouldMarkAtTheLatestDayMinimum()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");

        var aggregates = SwingTestData
            .Steady(
                a,
                Day0.AddDays(-SwingMarketData.TypicalVolumeDays),
                Day0.AddDays(-1),
                10m,
                10_000m
            )
            .Append(new DailyTradeAggregate(a.Id, Day0, 10m, 8m, 12m, 10_000m));

        var market = MarketData([a], aggregates);
        var signals = Constant(2, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(market, signals, Configuration(), 2);

        // Assert
        var position = result.Positions.ShouldHaveSingleItem();
        position.ExitPrice.ShouldBe(8m);
        position.ProfitLoss.ShouldBe(-200m);
        result.FinalBalance.ShouldBe(800m);
    }

    [Fact]
    public void Run_WhenTradingThroughMovingPrices_ShouldKeepCashConsistentWithClosedPositions()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var b = SwingTestData.CreateSymbol(_market, "B");

        var aggregates = Enumerable
            .Range(-SwingMarketData.TypicalVolumeDays, SwingMarketData.TypicalVolumeDays + 11)
            .SelectMany(i =>
                new[]
                {
                    new DailyTradeAggregate(
                        a.Id,
                        Day0.AddDays(i),
                        20m + i,
                        19m + i,
                        21m + i,
                        5_000m
                    ),
                    new DailyTradeAggregate(
                        b.Id,
                        Day0.AddDays(i),
                        40m - i,
                        39m - i,
                        41m - i,
                        5_000m
                    ),
                }
            );

        var market = SwingTestData.CreateMarket(SwingTestData.OsrsTax);
        var data = SwingMarketData.FromRaw(market, [a, b], [.. aggregates]);

        var signals = SwingTestData.Signals(
            10,
            [a, b],
            (day, s) =>
                SwingTestData.Signal(s == a ? 0.2m + (day % 3 * 0.3m) : 0.8m - (day % 2 * 0.7m))
        );

        // Act
        var result = Run(
            data,
            signals,
            Configuration(
                buyThreshold: 0.3m,
                sellThreshold: 0.2m,
                maxPositionPercent: 0.5m,
                holdPeriodDays: 3
            ),
            10,
            budget: 10_000m,
            participationRate: 0.1m,
            slippageMultiplier: 0.05m
        );

        // Assert
        result.Positions.Count.ShouldBeGreaterThan(2);
        result.FinalBalance.ShouldBe(10_000m + result.Positions.Sum(p => p.ProfitLoss), 0.000001m);
        result.Positions.ShouldAllBe(p => p.ExitTime >= p.EntryTime);
        result.Equity.Values.Count.ShouldBe(11);
    }

    [Fact]
    public void Run_WhenBuyingAtTheAveragePrice_ShouldRecordEquityAtTheBudgetOnTheFirstDay()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 3, 10m, 10_000m));
        var signals = Constant(3, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });

        // Act
        var result = Run(market, signals, Configuration(), 3);

        // Assert
        result.Equity.Values.Count.ShouldBe(4);
        result.Equity.Values.ShouldAllBe(v => v == 1000m);
        result.Equity.MaxDrawdown.ShouldBe(0m);
    }

    [Fact]
    public void Run_WhenNothingScoresAboveTheBuyThreshold_ShouldKeepTheBudget()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 2, 10m, 10_000m));
        var signals = Constant(2, [a], new Dictionary<string, decimal> { ["A"] = 0.3m });

        // Act
        var result = Run(market, signals, Configuration(buyThreshold: 0.3m), 2);

        // Assert
        result.Positions.ShouldBeEmpty();
        result.FinalBalance.ShouldBe(1000m);
    }

    [Fact]
    public async Task Run_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 2, 10m, 10_000m));
        var signals = Constant(2, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // Act
        var run = () =>
            Run(market, signals, Configuration(), 2, cancellationToken: cancellation.Token);

        // Assert
        run.ShouldThrow<OperationCanceledException>();
    }

    [Fact]
    public void Run_ShouldReportProgressFromZeroToOne()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var market = MarketData([a], SwingTestData.Steady(a, 2, 10m, 10_000m));
        var signals = Constant(2, [a], new Dictionary<string, decimal> { ["A"] = 0.9m });
        var progress = Substitute.For<IProgress<double>>();

        // Act
        Run(market, signals, Configuration(), 2, progress: progress);

        // Assert
        progress.Received(1).Report(0d);
        progress.Received(1).Report(1d);
        progress.DidNotReceive().Report(Arg.Is<double>(p => p < 0d || p > 1d));
    }

    [Fact]
    public void PlanEntries_ShouldTakeCandidatesAboveThresholdBestFirstSkippingHeldOnes()
    {
        // Arrange
        var candidates = Candidates(
            ("A", 0.9m),
            ("B", 0.5m),
            ("C", 0.2m),
            ("D", 0.95m),
            ("E", 0.3m)
        );

        var held = candidates.Single(c => c.Symbol.Name == "D").Symbol.Id;

        // Act
        var entries = SwingSimulator
            .PlanEntries(
                candidates,
                Configuration(buyThreshold: 0.3m),
                id => id == held,
                1,
                1000m,
                SizeAll
            )
            .ToList();

        // Assert
        entries.Select(e => e.Candidate.Symbol.Name).ShouldBe(["A", "B"]);
    }

    [Fact]
    public void PlanEntries_WhenHeldCountReachesMaxPositions_ShouldStop()
    {
        // Arrange
        var candidates = Candidates(("A", 0.9m), ("B", 0.5m), ("C", 0.4m));

        // Act
        var entries = SwingSimulator
            .PlanEntries(candidates, Configuration(maxPositions: 3), _ => false, 2, 1000m, SizeAll)
            .ToList();

        // Assert
        entries.ShouldHaveSingleItem().Candidate.Symbol.Name.ShouldBe("A");
    }

    [Fact]
    public void PlanEntries_ShouldSizeEachEntryFromTheCashLeftAfterPreviousEntries()
    {
        // Arrange
        var candidates = Candidates(("A", 0.9m), ("B", 0.5m));
        var budgets = new List<decimal>();

        // Act
        SwingSimulator
            .PlanEntries(
                candidates,
                Configuration(maxPositionPercent: 0.5m),
                _ => false,
                0,
                1000m,
                (candidate, budget) =>
                {
                    budgets.Add(budget);
                    return SizeAll(candidate, budget);
                }
            )
            .ToList();

        // Assert
        budgets.ShouldBe([500m, 250m]);
    }

    [Fact]
    public void PlanEntries_WhenMaxPositionPercentExceedsOne_ShouldCapTheShareAtAllCash()
    {
        // Arrange
        var candidates = Candidates(("A", 0.9m));
        var budgets = new List<decimal>();

        // Act
        SwingSimulator
            .PlanEntries(
                candidates,
                Configuration(maxPositionPercent: 2m),
                _ => false,
                0,
                1000m,
                (candidate, budget) =>
                {
                    budgets.Add(budget);
                    return SizeAll(candidate, budget);
                }
            )
            .ToList();

        // Assert
        budgets.ShouldBe([1000m]);
    }

    [Fact]
    public void PlanEntries_WhenACandidateCannotBeSized_ShouldNotUseUpAPositionSlot()
    {
        // Arrange
        var candidates = Candidates(("A", 0.9m), ("B", 0.5m));

        // Act
        var entries = SwingSimulator
            .PlanEntries(
                candidates,
                Configuration(maxPositions: 1),
                _ => false,
                0,
                1000m,
                (candidate, budget) =>
                    candidate.Symbol.Name == "A" ? null : SizeAll(candidate, budget)
            )
            .ToList();

        // Assert
        entries.ShouldHaveSingleItem().Candidate.Symbol.Name.ShouldBe("B");
    }

    private List<SwingCandidate> Candidates(params (string Name, decimal Score)[] candidates)
    {
        return
        [
            .. candidates.Select(c => new SwingCandidate(
                SwingTestData.CreateSymbol(_market, c.Name),
                c.Score,
                10m
            )),
        ];
    }

    private static SwingEntry SizeAll(SwingCandidate candidate, decimal budget)
    {
        return new SwingEntry(
            candidate,
            Math.Floor(budget / candidate.Price),
            candidate.Price,
            PositionLimit.Budget
        );
    }
}
