using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization;

public sealed class ParameterChromosomeTests
{
    private const int Repetitions = 200;

    private readonly ParameterSpace _space = new(
        [
            new Parameter("lookbackHours", 4, 72, IsWhole: true),
            new Parameter("bidOffset", -0.3, 0.4),
            new Parameter("minMargin", 0, 0.15),
        ],
        [24, 0.1, 0.05],
        genes => new FlipConfiguration(
            LookbackHours: (int)genes[0],
            BidOffset: ParameterSpace.ToDecimal(genes[1]),
            MinMargin: ParameterSpace.ToDecimal(genes[2])
        )
    );

    [Fact]
    public void Constructor_ShouldStartFromTheSeed()
    {
        // Act
        var chromosome = new ParameterChromosome(_space);

        // Assert
        chromosome.Space.ShouldBeSameAs(_space);
        chromosome.Genes.ShouldBe([24d, 0.1d, 0.05d]);
    }

    [Fact]
    public void Genes_ShouldReturnACopy()
    {
        // Arrange
        var chromosome = new ParameterChromosome(_space);

        // Act
        chromosome.Genes[0] = 70;

        // Assert
        chromosome.Genes[0].ShouldBe(24d);
    }

    [Fact]
    public void Configuration_ShouldBeBuiltFromTheGenes()
    {
        // Arrange
        var chromosome = new ParameterChromosome(_space);

        // Act
        var configuration = chromosome.Configuration;

        // Assert
        configuration.ShouldBe(
            new FlipConfiguration(LookbackHours: 24, BidOffset: 0.1m, MinMargin: 0.05m)
        );
    }

    [Fact]
    public void CreateRandom_ShouldStayWithinBoundsAndRoundWholeParameters()
    {
        // Act
        var chromosomes = Enumerable
            .Range(0, Repetitions)
            .Select(_ => ParameterChromosome.CreateRandom(_space))
            .ToList();

        // Assert
        chromosomes.ShouldAllBe(c => WithinBounds(c));
        chromosomes.ShouldAllBe(c => c.Genes[0] == Math.Round(c.Genes[0]));
        chromosomes.Select(c => c.Genes[1]).Distinct().Count().ShouldBeGreaterThan(1);
    }

    [Fact]
    public void Mutate_ShouldChangeGenesButStayWithinBounds()
    {
        // Arrange
        var chromosome = new ParameterChromosome(_space);

        // Act
        for (var i = 0; i < Repetitions; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        WithinBounds(chromosome).ShouldBeTrue();
        chromosome.Genes.ShouldNotBe([24d, 0.1d, 0.05d]);
        chromosome.Genes[0].ShouldBe(Math.Round(chromosome.Genes[0]));
    }

    [Fact]
    public void Mutate_WhenRateIsZero_ShouldStillMutateAboutOneGenePerCall()
    {
        // Arrange
        var chromosome = new ParameterChromosome(_space);

        // Act
        for (var i = 0; i < Repetitions; i++)
        {
            chromosome.Mutate(0);
        }

        // Assert
        chromosome.Genes.ShouldNotBe([24d, 0.1d, 0.05d]);
        WithinBounds(chromosome).ShouldBeTrue();
    }

    [Fact]
    public void Mutate_WhenTheSpaceHasNoParameters_ShouldDoNothing()
    {
        // Arrange
        var space = new ParameterSpace([], [], _ => new FlipConfiguration());
        var chromosome = new ParameterChromosome(space);

        // Act
        chromosome.Mutate(1.0);

        // Assert
        chromosome.Genes.ShouldBeEmpty();
        chromosome.Configuration.ShouldBe(new FlipConfiguration());
    }

    [Fact]
    public void Crossover_ShouldTakeEveryGeneFromOneOfTheParents()
    {
        // Arrange
        var mother = new ParameterChromosome(_space);
        var father = ParameterChromosome.CreateRandom(_space);

        // Act
        var children = Enumerable
            .Range(0, Repetitions)
            .Select(_ => mother.Crossover(father).ShouldBeOfType<ParameterChromosome>())
            .ToList();

        // Assert
        children.ShouldAllBe(c => c.Space == _space);
        foreach (var child in children)
        {
            for (var i = 0; i < child.Genes.Length; i++)
            {
                child.Genes[i].ShouldBeOneOf(mother.Genes[i], father.Genes[i]);
            }
        }
    }

    [Fact]
    public void Crossover_WhenPartnerComesFromAnotherSpace_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var other = new ParameterSpace(
            [new Parameter("lookbackHours", 4, 72, IsWhole: true)],
            [24],
            genes => new FlipConfiguration(LookbackHours: (int)genes[0])
        );

        var chromosome = new ParameterChromosome(_space);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            chromosome.Crossover(new ParameterChromosome(other))
        );
    }

    [Fact]
    public void Crossover_WhenPartnerIsNotAParameterChromosome_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var chromosome = new ParameterChromosome(_space);
        var partner = Substitute.For<IChromosome<double>>();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => chromosome.Crossover(partner));
    }

    private bool WithinBounds(ParameterChromosome chromosome)
    {
        return chromosome
            .Genes.Select((gene, i) => (Gene: gene, Parameter: _space.Parameters[i]))
            .All(g => g.Gene >= g.Parameter.Min && g.Gene <= g.Parameter.Max);
    }
}
