using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Algorithms.Genetic;

public sealed class GeneticAlgorithmEngineTests
{
    [Fact]
    public void EvaluateFitness_ShouldReturnExpectedValue()
    {
        // Arrange
        var fixture = new Fixture();
        var fitnessComponents = fixture.CreateMany<FitnessComponent<bool>>().ToList();
        var engine = new GeneticAlgorithmEngine<bool>(1, 1, 1, fitnessComponents);
        var chromosome = fixture.Create<TestChromosome>();

        // Act
        var actualFitness = engine.EvaluateFitness(chromosome);

        // Assert
        actualFitness.ShouldBe(
            fitnessComponents.Sum(c => c.Weight * c.FitnessFunction(chromosome))
        );
    }

    [Fact]
    public void Evolve_WhenNoTargetFitnessSpecified_ShouldReturnNewChromosome()
    {
        // Arrange
        const int populationSize = 3;
        const double mutationRate = 0.01;
        const double elitismRate = 0;

        var fixture = new Fixture();
        var fitnessComponents = fixture.CreateMany<FitnessComponent<bool>>().ToList();
        var engine = new GeneticAlgorithmEngine<bool>(
            populationSize,
            mutationRate,
            elitismRate,
            fitnessComponents
        );
        var population = fixture
            .CreateMany<TestChromosome>(populationSize)
            .ToArray<IChromosome<bool>>();

        // Act
        var actualChromosome = engine.Evolve(population, 2);

        // Assert
        actualChromosome.ShouldNotBeOneOf(population);
    }

    [Fact]
    public void Evolve_WhenTargetFitnessSpecified_ShouldReturnImmediately()
    {
        // Arrange
        const int populationSize = 3;
        const double mutationRate = 0.01;
        const double elitismRate = 0;

        var fixture = new Fixture();
        var fitnessComponents = fixture.CreateMany<FitnessComponent<bool>>().ToList();
        var engine = new GeneticAlgorithmEngine<bool>(
            populationSize,
            mutationRate,
            elitismRate,
            fitnessComponents
        );
        var population = fixture
            .CreateMany<TestChromosome>(populationSize)
            .ToArray<IChromosome<bool>>();

        // Act
        var actualChromosome = engine.Evolve(population, 2, 0);

        // Assert
        actualChromosome.ShouldBeOneOf(population);
    }
}
