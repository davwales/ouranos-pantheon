using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Algorithms.Genetic;

public sealed class GeneticAlgorithmBuilderTests
{
    private readonly GeneticAlgorithmBuilder<bool> _builder = new();

    [Fact]
    public void SetElitismRate_WhenValid_ShouldSetElitismRate()
    {
        // Arrange
        const double elitismRate = 0.1234;

        // Act
        _builder.SetElitismRate(elitismRate);

        // Assert
        _builder.ElitismRate.ShouldBe(elitismRate);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.01)]
    public void SetElitismRate_WhenOutOfRange_ShouldThrowArgumentOutOfRangeException(
        double elitismRate
    )
    {
        // Act
        var set = () => _builder.SetElitismRate(elitismRate);

        // Assert
        set.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void SetMutationRate_WhenValid_ShouldSetMutationRate()
    {
        // Arrange
        const double mutationRate = 0.1234;

        // Act
        _builder.SetMutationRate(mutationRate);

        // Assert
        _builder.MutationRate.ShouldBe(mutationRate);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.01)]
    public void SetMutationRate_WhenOutOfRange_ShouldThrowArgumentOutOfRangeException(
        double mutationRate
    )
    {
        // Act
        var set = () => _builder.SetMutationRate(mutationRate);

        // Assert
        set.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPopulationSize_ShouldSetPopulationSize()
    {
        // Arrange
        const uint populationSize = 123;

        // Act
        _builder.SetPopulationSize(populationSize);

        // Assert
        _builder.PopulationSize.ShouldBe(populationSize);
    }

    [Fact]
    public void Build_WhenNoFitnessComponentProvided_ShouldThrowInvalidOperationException()
    {
        // Act
        var build = () => _builder.Build();

        // Assert
        var actualException = build.ShouldThrow<InvalidOperationException>();
        actualException.Message.ShouldBe("No Fitness Components were found.");
    }

    [Fact]
    public void Build_WhenFitnessComponentProvided_ShouldReturnEngine()
    {
        // Arrange
        _builder.AddFitnessComponent(_ => 123.4);

        // Act
        var build = () => _builder.Build();

        // Assert
        var engine = build.ShouldNotThrow();
        engine.ShouldBeOfType<GeneticAlgorithmEngine<bool>>();
    }

    [Fact]
    public void AddFitnessComponent_WhenUnweighted_ShouldAddFitnessComponent()
    {
        // Arrange
        Func<IChromosome<bool>, double> func = _ => 123.4;

        // Act
        _builder.AddFitnessComponent(func);

        // Assert
        _builder.FitnessComponents.Count.ShouldBe(1);
        _builder.FitnessComponents[0].ShouldBe(new FitnessComponent<bool>(1, func));
    }

    [Fact]
    public void AddFitnessComponent_WhenWeighted_ShouldSetFitnessComponent()
    {
        // Arrange
        Func<IChromosome<bool>, double> func = _ => 123.4;

        // Act
        _builder.AddFitnessComponent(45.6, func);

        // Assert
        _builder.FitnessComponents.Count.ShouldBe(1);
        _builder.FitnessComponents[0].ShouldBe(new FitnessComponent<bool>(45.6, func));
    }
}
