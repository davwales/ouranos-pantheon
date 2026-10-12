using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class OptimizationFitnessTests
{
    private const decimal Budget = 1000m;
    private const double DrawdownWeight = 0.5;

    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static BacktestPosition Position(string symbolId, decimal profitLoss)
    {
        return new BacktestPosition { SymbolId = symbolId, ProfitLoss = profitLoss };
    }

    [Fact]
    public void Compute_WhenProfitComesFromOneSymbol_ShouldDiscountItsExcessShare()
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
        var concentratedFitness = OptimizationFitness.Compute(
            results,
            concentrated,
            Budget,
            DrawdownWeight,
            0
        );
        var diversifiedFitness = OptimizationFitness.Compute(
            results,
            diversified,
            Budget,
            DrawdownWeight,
            0
        );

        // Assert
        concentratedFitness.ShouldBe(0.105, 0.0001);
        diversifiedFitness.ShouldBe(0.0825, 0.0001);
    }

    [Fact]
    public void Compute_WhenDrawdownHigher_ShouldPenalizeByDrawdownWeight()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", 10m), Position("B", 10m)];
        var calm = new BacktestResults { TotalTrades = 2, MaxDrawdownPercent = 0.1m };
        var volatileResults = new BacktestResults { TotalTrades = 2, MaxDrawdownPercent = 0.3m };

        // Act
        var calmFitness = OptimizationFitness.Compute(calm, positions, Budget, DrawdownWeight, 0);
        var volatileFitness = OptimizationFitness.Compute(
            volatileResults,
            positions,
            Budget,
            DrawdownWeight,
            0
        );

        // Assert
        (calmFitness - volatileFitness).ShouldBe(0.1, 0.0001);
    }

    [Fact]
    public void Compute_WhenTotalTradesBelowMinTrades_ShouldApplyUnderTradingPenalty()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", 10m), Position("B", 10m)];
        var results = new BacktestResults { TotalTrades = 2 };

        // Act
        var fitness = OptimizationFitness.Compute(results, positions, Budget, DrawdownWeight, 5);

        // Assert
        fitness.ShouldBe(0.015 - 0.03, 0.0001);
    }

    [Fact]
    public void Compute_WhenTotalTradesMeetMinTrades_ShouldNotPenalize()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", 10m), Position("B", 10m)];
        var results = new BacktestResults { TotalTrades = 5 };

        // Act
        var fitness = OptimizationFitness.Compute(results, positions, Budget, DrawdownWeight, 5);

        // Assert
        fitness.ShouldBe(0.015, 0.0001);
    }

    [Theory]
    [InlineData(0, 30, 0)]
    [InlineData(-3, 30, 0)]
    [InlineData(10, 30, 10)]
    [InlineData(10, 15, 5)]
    [InlineData(10, 7, 2)]
    [InlineData(1, 3, 1)]
    [InlineData(10, 90, 30)]
    public void MinTradesForSegment_ShouldScaleMonthlyTargetToSegmentLength(
        int minTradesPerMonth,
        int segmentDays,
        int expected
    )
    {
        // Act
        var result = OptimizationFitness.MinTradesForSegment(minTradesPerMonth, segmentDays);

        // Assert
        result.ShouldBe(expected);
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
        var result = OptimizationFitness.ConcentrationAdjustedReturn(positions, Budget);

        // Assert
        result.ShouldBe(0.0625m);
    }

    [Fact]
    public void ConcentrationAdjustedReturn_WhenEverySymbolLoses_ShouldKeepAllLosses()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", -10m), Position("B", -20m)];

        // Act
        var result = OptimizationFitness.ConcentrationAdjustedReturn(positions, Budget);

        // Assert
        result.ShouldBe(-0.03m);
    }

    [Fact]
    public void ConcentrationAdjustedReturn_WhenNoPositions_ShouldReturnZero()
    {
        // Act
        var result = OptimizationFitness.ConcentrationAdjustedReturn([], Budget);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void ConcentrationAdjustedReturn_WhenBudgetNotPositive_ShouldReturnZero()
    {
        // Arrange
        List<BacktestPosition> positions = [Position("A", 10m)];

        // Act
        var result = OptimizationFitness.ConcentrationAdjustedReturn(positions, 0m);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void CombineSegments_ShouldWeightMeanAndWorstSegment()
    {
        // Act
        var result = OptimizationFitness.CombineSegments([0.04, 0.02, -0.03]);

        // Assert
        result.ShouldBe((0.75 * 0.01) + (0.25 * -0.03), 0.0001);
    }

    [Fact]
    public void BuildSegments_ShouldSplitWindowIntoConsecutiveWholeDaySegments()
    {
        // Arrange
        var end = Start.AddDays(95);

        // Act
        var segments = OptimizationFitness.BuildSegments(Start, end, 30);

        // Assert
        segments.Count.ShouldBe(3);
        segments[0].Start.ShouldBe(Start);
        segments[^1].End.ShouldBe(end);
        segments.Zip(segments.Skip(1)).ShouldAllBe(p => p.First.End == p.Second.Start);
        segments.ShouldAllBe(s => (s.End - s.Start).TotalDays % 1 == 0);
    }

    [Fact]
    public void BuildSegments_WhenSegmentDaysShorter_ShouldReturnMoreSegments()
    {
        // Act
        var segments = OptimizationFitness.BuildSegments(Start, Start.AddDays(28), 7);

        // Assert
        segments.Count.ShouldBe(4);
        segments.ShouldAllBe(s => (s.End - s.Start).TotalDays == 7);
    }

    [Fact]
    public void BuildSegments_WhenWindowShorterThanOneSegment_ShouldReturnSingleSegment()
    {
        // Act
        var segments = OptimizationFitness.BuildSegments(Start, Start.AddDays(10), 30);

        // Assert
        var segment = segments.ShouldHaveSingleItem();
        segment.Start.ShouldBe(Start);
        segment.End.ShouldBe(Start.AddDays(10));
    }

    [Theory]
    [InlineData(0.2, 80)]
    [InlineData(0.5, 50)]
    [InlineData(0.9, 50)]
    [InlineData(0.01, 95)]
    public void InSampleEnd_ShouldHoldBackClampedShareOfWindow(double ratio, int expectedDays)
    {
        // Act
        var result = OptimizationFitness.InSampleEnd(Start, Start.AddDays(100), ratio);

        // Assert
        result.ShouldBe(Start.AddDays(expectedDays));
    }

    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(10, 0, false)]
    [InlineData(10, -5, false)]
    [InlineData(-10, 5, false)]
    public void IsValidated_ShouldRequireProfitInAndOutOfSample(
        int inSampleReturn,
        int outSampleReturn,
        bool expected
    )
    {
        // Arrange
        var inSample = new BacktestResults { TotalReturn = inSampleReturn };
        var outSample = new BacktestResults { TotalReturn = outSampleReturn };

        // Act
        var result = OptimizationFitness.IsValidated(inSample, outSample);

        // Assert
        result.ShouldBe(expected);
    }
}
