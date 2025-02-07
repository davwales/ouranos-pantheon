using AutoFixture;
using Ouranos.Pantheon.Core.Common.GeneticAlgorithms;
using Shouldly;
using Xunit;

namespace Ouranos.Pantheon.Core.Common.Tests.GeneticAlgorithms;

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