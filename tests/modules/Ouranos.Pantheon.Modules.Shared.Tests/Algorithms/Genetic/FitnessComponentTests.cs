using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Algorithms.Genetic;

public sealed class FitnessComponentTests
{
    [Fact]
    public void Constructor_ShouldSetExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedWeight = fixture.Create<double>();
        var expectedFitnessFunction = fixture.Create<Func<IChromosome<TestChromosome>, double>>();

        // Act
        var actualFitnessComponent = new FitnessComponent<TestChromosome>(
            expectedWeight,
            expectedFitnessFunction
        );

        // Assert
        actualFitnessComponent.Weight.ShouldBe(expectedWeight);
        actualFitnessComponent.FitnessFunction.ShouldBe(expectedFitnessFunction);
    }
}