using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class ComputeFitnessTests
{
    private readonly IFixture _fixture = new Fixture();

    public ComputeFitnessTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    private OptimizeStrategyMessage DefaultMessage(int minTrades = 5)
    {
        return new(
            _fixture.Create<Id<Backtest>>(),
            Generations: 1,
            PopulationSize: 2,
            MinTrades: minTrades
        );
    }

    [Fact]
    public void ComputeFitness_WhenMetricsEqual_LowerWeightMagnitudeWins()
    {
        // Arrange
        var results = new BacktestResults
        {
            SharpeRatio = 2m,
            TotalReturnPercent = 20m,
            MaxDrawdownPercent = -10m,
            TotalTrades = 10,
        };
        var message = DefaultMessage();
        var smallWeights = new List<InputWeight> { new(InputKind.SignalTaxAdjustedRoi, 1m) };
        var largeWeights = new List<InputWeight> { new(InputKind.SignalTaxAdjustedRoi, 3m) };

        // Act
        var smallFitness = OptimizeStrategyConsumer.ComputeFitness(results, message, smallWeights);
        var largeFitness = OptimizeStrategyConsumer.ComputeFitness(results, message, largeWeights);

        // Assert
        smallFitness.ShouldBeGreaterThan(largeFitness);
        (smallFitness - largeFitness).ShouldBe(0.1, 0.0001);
    }

    [Fact]
    public void ComputeFitness_WhenSortinoIsNull_FallsBackToSharpe()
    {
        // Arrange
        var sharpeOnly = new BacktestResults { SharpeRatio = 1.5m, TotalTrades = 10 };
        var explicitSortino = new BacktestResults
        {
            SharpeRatio = 0m,
            SortinoRatio = 1.5m,
            TotalTrades = 10,
        };
        var message = DefaultMessage();
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var sharpeFitness = OptimizeStrategyConsumer.ComputeFitness(sharpeOnly, message, weights);
        var sortinoFitness = OptimizeStrategyConsumer.ComputeFitness(
            explicitSortino,
            message,
            weights
        );

        // Assert
        sharpeFitness.ShouldBe(sortinoFitness, 0.0001);
    }

    [Fact]
    public void ComputeFitness_WhenCagrIsNull_FallsBackToTotalReturnPercent()
    {
        // Arrange
        var returnOnly = new BacktestResults { TotalReturnPercent = 12m, TotalTrades = 10 };
        var explicitCagr = new BacktestResults { Cagr = 12m, TotalTrades = 10 };
        var message = DefaultMessage();
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var returnFitness = OptimizeStrategyConsumer.ComputeFitness(returnOnly, message, weights);
        var cagrFitness = OptimizeStrategyConsumer.ComputeFitness(explicitCagr, message, weights);

        // Assert
        returnFitness.ShouldBe(cagrFitness, 0.0001);
    }

    [Fact]
    public void ComputeFitness_WhenTurnoverRateHigher_PenaltyScalesLinearly()
    {
        // Arrange
        var lowTurnover = new BacktestResults { TotalTrades = 10, TurnoverRate = 0m };
        var highTurnover = new BacktestResults { TotalTrades = 10, TurnoverRate = 0.5m };
        var message = DefaultMessage();
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var lowFitness = OptimizeStrategyConsumer.ComputeFitness(lowTurnover, message, weights);
        var highFitness = OptimizeStrategyConsumer.ComputeFitness(highTurnover, message, weights);

        // Assert
        (lowFitness - highFitness).ShouldBe(0.05, 0.0001);
    }

    [Fact]
    public void ComputeFitness_WhenTotalTradesBelowMinTrades_AppliesUnderTradingPenalty()
    {
        // Arrange
        var underTrading = new BacktestResults
        {
            SharpeRatio = 1m,
            TotalReturnPercent = 10m,
            MaxDrawdownPercent = -5m,
            TotalTrades = 2,
        };
        var sufficient = underTrading with { TotalTrades = 5 };
        var message = DefaultMessage(minTrades: 5);
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var underFitness = OptimizeStrategyConsumer.ComputeFitness(underTrading, message, weights);
        var sufficientFitness = OptimizeStrategyConsumer.ComputeFitness(
            sufficient,
            message,
            weights
        );

        // Assert
        underFitness.ShouldBeLessThan(sufficientFitness);
        (sufficientFitness - underFitness).ShouldBe(0.3, 0.0001);
    }

    [Fact]
    public void ComputeFitness_WhenTotalTradesMeetsMinTrades_AppliesNoUnderTradingPenalty()
    {
        // Arrange
        var results = new BacktestResults
        {
            SharpeRatio = 1m,
            TotalReturnPercent = 10m,
            MaxDrawdownPercent = -5m,
            TotalTrades = 5,
        };
        var message = DefaultMessage(minTrades: 5);
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var fitness = OptimizeStrategyConsumer.ComputeFitness(results, message, weights);

        // Assert
        fitness.ShouldBe(5.85, 0.0001);
    }
}
