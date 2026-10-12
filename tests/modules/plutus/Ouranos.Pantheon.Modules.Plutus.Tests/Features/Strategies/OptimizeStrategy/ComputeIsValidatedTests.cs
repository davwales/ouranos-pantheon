using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class ComputeIsValidatedTests
{
    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(10, 0, false)]
    [InlineData(10, -5, false)]
    [InlineData(-10, 5, false)]
    public void ComputeIsValidated_ShouldRequireProfitInAndOutOfSample(
        int inSampleReturn,
        int outSampleReturn,
        bool expected
    )
    {
        // Arrange
        var inSample = new BacktestResults { TotalReturn = inSampleReturn };
        var outSample = new BacktestResults { TotalReturn = outSampleReturn };

        // Act
        var result = OptimizeStrategyConsumer.ComputeIsValidated(inSample, outSample);

        // Assert
        result.ShouldBe(expected);
    }
}
