using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class ComputeFitnessTests
{
    private const decimal Budget = 1000m;

    private readonly Fixture _fixture = new();

    public ComputeFitnessTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    private OptimizeStrategyMessage DefaultMessage(int minTrades = 0, double drawdownWeight = 0.5)
    {
        return new(
            _fixture.Create<Id<Backtest>>(),
            Generations: 1,
            PopulationSize: 2,
            DrawdownWeight: drawdownWeight,
            MinTradesPerMonth: minTrades
        );
    }

    private static BacktestPosition Position(string symbolId, decimal profitLoss)
    {
        return new BacktestPosition { SymbolId = symbolId, ProfitLoss = profitLoss };
    }

    [Fact]
    public void ComputeFitness_WhenProfitComesFromOneSymbol_ShouldDiscountItsExcessShare()
    {
        // Arrange
        var results = new BacktestResults { TotalTrades = 3 };

        List<BacktestPosition> concentrated =
        [
            Position("A", 500m),
            Position("B", -10m),
            Position("C", -10m),
        ];

        List<BacktestPosition> diversified =
        [
            Position("A", 30m),
            Position("B", 30m),
            Position("C", 30m),
        ];

        // Act
        var concentratedFitness = OptimizeStrategyConsumer.ComputeFitness(
            results,
            concentrated,
            Budget,
            DefaultMessage()
        );

        var diversifiedFitness = OptimizeStrategyConsumer.ComputeFitness(
            results,
            diversified,
            Budget,
            DefaultMessage()
        );

        // Assert
        concentratedFitness.ShouldBe(0.105, 0.0001);
        diversifiedFitness.ShouldBe(0.0825, 0.0001);
    }

    [Fact]
    public void ComputeFitness_WhenDrawdownHigher_ShouldPenalizeByDrawdownWeight()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", 10m), Position("B", 10m)];
        var calm = new BacktestResults { TotalTrades = 2, MaxDrawdownPercent = 0.1m };
        var volatile_ = new BacktestResults { TotalTrades = 2, MaxDrawdownPercent = 0.3m };

        // Act
        var calmFitness = OptimizeStrategyConsumer.ComputeFitness(
            calm,
            positions,
            Budget,
            DefaultMessage()
        );

        var volatileFitness = OptimizeStrategyConsumer.ComputeFitness(
            volatile_,
            positions,
            Budget,
            DefaultMessage()
        );

        // Assert
        (calmFitness - volatileFitness).ShouldBe(0.1, 0.0001);
    }

    [Fact]
    public void ComputeFitness_WhenTotalTradesBelowMinTrades_ShouldApplyUnderTradingPenalty()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", 10m), Position("B", 10m)];
        var results = new BacktestResults { TotalTrades = 2 };

        // Act
        var fitness = OptimizeStrategyConsumer.ComputeFitness(
            results,
            positions,
            Budget,
            DefaultMessage(minTrades: 5)
        );

        // Assert
        fitness.ShouldBe(0.015 - 0.03, 0.0001);
    }

    [Fact]
    public void ConcentrationAdjustedReturn_ShouldSumAcrossPartialPositionsPerSymbol()
    {
        // Arrange
        List<BacktestPosition> positions =
        [
            Position("A", 40m),
            Position("A", 40m),
            Position("B", 50m),
            Position("C", -20m),
        ];

        // Act
        var result = OptimizeStrategyConsumer.ConcentrationAdjustedReturn(positions, Budget);

        // Assert
        result.ShouldBe(0.0625m);
    }

    [Fact]
    public void ConcentrationAdjustedReturn_WhenEverySymbolLoses_ShouldKeepAllLosses()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", -10m), Position("B", -20m)];

        // Act
        var result = OptimizeStrategyConsumer.ConcentrationAdjustedReturn(positions, Budget);

        // Assert
        result.ShouldBe(-0.03m);
    }

    [Fact]
    public void ConcentrationAdjustedReturn_WhenNoPositions_ShouldReturnZero()
    {
        // Act
        var result = OptimizeStrategyConsumer.ConcentrationAdjustedReturn([], Budget);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void CombineSegmentFitness_ShouldWeightMeanAndWorstSegment()
    {
        // Act
        var result = OptimizeStrategyConsumer.CombineSegmentFitness([0.04, 0.02, -0.03]);

        // Assert
        result.ShouldBe((0.75 * 0.01) + (0.25 * -0.03), 0.0001);
    }

    [Fact]
    public void BuildSegments_ShouldSplitWindowIntoConsecutiveWholeDaySegments()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(95);

        // Act
        var segments = OptimizeStrategyConsumer.BuildSegments(start, end);

        // Assert
        segments.Count.ShouldBe(3);
        segments[0].Start.ShouldBe(start);
        segments[^1].End.ShouldBe(end);
        segments.Zip(segments.Skip(1)).ShouldAllBe(p => p.First.End == p.Second.Start);
        segments.Select(s => s.ScoreCache).Distinct().Count().ShouldBe(3);
    }

    [Fact]
    public void BuildSegments_WhenWindowShorterThanOneSegment_ShouldReturnSingleSegment()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var segments = OptimizeStrategyConsumer.BuildSegments(start, start.AddDays(10));

        // Assert
        segments.ShouldHaveSingleItem();
    }
}
