using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class ComputeIsValidatedTests
{
    [Fact]
    public void ComputeIsValidated_WhenBothSharpesNonZeroAndOosAtLeastHalfOfIs_ReturnsTrue()
    {
        // Arrange
        var inSample = new BacktestResults { SharpeRatio = 2m };
        var outSample = new BacktestResults { SharpeRatio = 1m };

        // Act
        var result = OptimizeStrategyConsumer.ComputeIsValidated(inSample, outSample);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ComputeIsValidated_WhenOosBelowHalfOfIs_ReturnsFalse()
    {
        // Arrange
        var inSample = new BacktestResults { SharpeRatio = 2m };
        var outSample = new BacktestResults { SharpeRatio = 0.9m };

        // Act
        var result = OptimizeStrategyConsumer.ComputeIsValidated(inSample, outSample);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ComputeIsValidated_WhenInSampleSharpeZero_ReturnsFalse()
    {
        // Arrange
        var inSample = new BacktestResults { SharpeRatio = 0m };
        var outSample = new BacktestResults { SharpeRatio = 1m };

        // Act
        var result = OptimizeStrategyConsumer.ComputeIsValidated(inSample, outSample);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ComputeIsValidated_WhenOutSampleSharpeZero_ReturnsFalse()
    {
        // Arrange
        var inSample = new BacktestResults { SharpeRatio = 2m };
        var outSample = new BacktestResults { SharpeRatio = 0m };

        // Act
        var result = OptimizeStrategyConsumer.ComputeIsValidated(inSample, outSample);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ComputeIsValidated_WhenNegativeSharpes_ComparedNumericallyOosGreaterThanHalfIsValidates()
    {
        // Arrange
        var inSample = new BacktestResults { SharpeRatio = -2m };
        var outSample = new BacktestResults { SharpeRatio = -1m };

        // Act
        var result = OptimizeStrategyConsumer.ComputeIsValidated(inSample, outSample);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ComputeIsValidated_WhenNegativeSharpesOosWorseThanHalfIs_ReturnsFalse()
    {
        // Arrange
        var inSample = new BacktestResults { SharpeRatio = -2m };
        var outSample = new BacktestResults { SharpeRatio = -2m };

        // Act
        var result = OptimizeStrategyConsumer.ComputeIsValidated(inSample, outSample);

        // Assert
        result.ShouldBeFalse();
    }
}
