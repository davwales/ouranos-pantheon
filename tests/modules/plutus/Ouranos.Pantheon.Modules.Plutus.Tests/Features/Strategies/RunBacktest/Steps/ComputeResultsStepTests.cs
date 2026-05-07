using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class ComputeResultsStepTests
{
    [Fact]
    public void ComputeResults_WhenAllWinners_CalculatesMetrics()
    {
        // Arrange
        const decimal budget = 10000m;
        const decimal balance = 12000m;
        const decimal maxDrawdown = 0.1m;
        var portfolioValues = new List<decimal> { 10500m, 11000m, 11500m, 12000m };

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
        var closedPositions = new List<BacktestPosition> { pos1, pos2 };

        // Act
        var results = ComputeResultsStep.ComputeResults(budget, balance, maxDrawdown, 12000m, closedPositions, portfolioValues, 10);

        // Assert
        results.TotalReturn.ShouldBe(2000m);
        results.TotalReturnPercent.ShouldBe(0.2m);
        results.WinningTrades.ShouldBe(2);
        results.LosingTrades.ShouldBe(0);
        results.TotalTrades.ShouldBe(2);
        results.WinRate.ShouldBe(1m);
        results.MaxDrawdown.ShouldBe(1200m);
        results.MaxDrawdownPercent.ShouldBe(0.1m);
        results.FinalBalance.ShouldBe(12000m);
    }

    [Fact]
    public void ComputeResults_WhenMixedTrades_CalculatesCorrectWinRate()
    {
        // Arrange
        const decimal budget = 10000m;
        const decimal balance = 10500m;
        const decimal maxDrawdown = 0m;
        var portfolioValues = new List<decimal> { 9500m, 10000m, 10500m };

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
        var closedPositions = new List<BacktestPosition> { winPos, lossPos };

        // Act
        var results = ComputeResultsStep.ComputeResults(budget, balance, maxDrawdown, 10500m, closedPositions, portfolioValues, 10);

        // Assert
        results.TotalReturn.ShouldBe(500m);
        results.WinningTrades.ShouldBe(1);
        results.LosingTrades.ShouldBe(1);
        results.WinRate.ShouldBe(0.5m);
        results.AverageTradeReturn.ShouldBe(0.15m);
    }

    [Fact]
    public void ComputeResults_WhenNoTrades_ReturnsZeroMetrics()
    {
        // Arrange
        const decimal budget = 10000m;
        const decimal balance = 10000m;
        const decimal maxDrawdown = 0m;
        var closedPositions = new List<BacktestPosition>();
        var portfolioValues = new List<decimal>();

        // Act
        var results = ComputeResultsStep.ComputeResults(budget, balance, maxDrawdown, 10000m, closedPositions, portfolioValues, 10);

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
        var result = ComputeResultsStep.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenConstantValues_ReturnsZero()
    {
        // Arrange
        var values = new List<decimal> { 10000m, 10000m, 10000m };

        // Act
        var result = ComputeResultsStep.ComputeSharpeRatio(values);

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
        var result = ComputeResultsStep.ComputeSharpeRatio(values);

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
        var result = ComputeResultsStep.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBeLessThan(0m);
    }

    [Fact]
    public void ComputeSharpeRatio_WhenTwoValues_ReturnsZero()
    {
        // Arrange
        var values = new List<decimal> { 100m, 200m };

        // Act
        var result = ComputeResultsStep.ComputeSharpeRatio(values);

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
        var result = ComputeResultsStep.ComputeSharpeRatio(values);

        // Assert
        result.ShouldBe(0m);
    }
}
