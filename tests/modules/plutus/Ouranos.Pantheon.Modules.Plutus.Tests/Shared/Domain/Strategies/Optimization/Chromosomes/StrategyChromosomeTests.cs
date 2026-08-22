using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class StrategyChromosomeTests
{
    private static readonly InputKind[] InputKinds = Enum.GetValues<InputKind>();

    private static StrategyChromosome ChromosomeWithAllCommonFields()
    {
        var config = new TradingConfiguration
        {
            MaxPositions = 5,
            MaxPositionPercent = 0.25m,
            HoldPeriodDays = 10,
        };

        var weights = new List<InputWeight> { new(InputKind.SignalTaxAdjustedRoi, 1m) };
        return new StrategyChromosome(config, weights, new InputThresholds());
    }

    [Fact]
    public void Genes_WhenCalled_ShouldReturnNonEmptyDoubleArray()
    {
        // Arrange
        IChromosome<double> chromosome = StrategyChromosome.CreateRandom();

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.ShouldBeOfType<double[]>();
        genes.ShouldNotBeEmpty();
        genes.ShouldAllBe(g => !double.IsNaN(g));
    }

    [Fact]
    public void Genes_WhenAllCommonFieldsSet_HasFixedLengthThreeCommonPlusSevenWeightsPlusTwoThresholds()
    {
        // Arrange
        var chromosome = ChromosomeWithAllCommonFields();

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(12);
    }

    [Fact]
    public void Genes_WhenCommonFieldsSet_ShouldSerializeThemFirstInOrder()
    {
        // Arrange
        var config = new TradingConfiguration
        {
            MaxPositions = 7,
            MaxPositionPercent = 0.25m,
            HoldPeriodDays = 12,
        };
        var chromosome = new StrategyChromosome(
            config,
            StrategyTestFactory.DefaultWeights(),
            new InputThresholds()
        );

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes[0].ShouldBe(7);
        genes[1].ShouldBe(0.25);
        genes[2].ShouldBe(12);
    }

    [Fact]
    public void Genes_WhenWeightsSet_LaysThemOutInEnumOrderAfterCommonFields()
    {
        // Arrange
        var config = new TradingConfiguration
        {
            MaxPositions = 1,
            MaxPositionPercent = 0.1m,
            HoldPeriodDays = 1,
        };

        var weights = new List<InputWeight>
        {
            new(InputKind.SignalPriceVelocity, 0.7m),
            new(InputKind.SignalTaxAdjustedRoi, 0.1m),
            new(InputKind.SignalRsi, 1.0m),
        };

        var chromosome = new StrategyChromosome(config, weights, new InputThresholds());

        // Act
        var genes = chromosome.Genes;

        // Assert
        for (var i = 0; i < InputKinds.Length; i++)
        {
            var kind = InputKinds[i];
            var expected = weights.FirstOrDefault(w => w.Kind == kind)?.Weight ?? 0m;
            genes[3 + i].ShouldBe((double)expected);
        }
    }

    [Fact]
    public void Configuration_WhenCreateRandom_ShouldHaveAllCommonFieldsWithinExpectedBounds()
    {
        // Arrange & Act
        var chromosome = StrategyChromosome.CreateRandom();

        // Assert
        var maxPositions = chromosome.Configuration.MaxPositions;
        maxPositions.ShouldNotBeNull();
        maxPositions.Value.ShouldBeInRange(1, 19);

        var maxPositionPercent = chromosome.Configuration.MaxPositionPercent;
        maxPositionPercent.ShouldNotBeNull();
        maxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);

        var holdPeriodDays = chromosome.Configuration.HoldPeriodDays;
        holdPeriodDays.ShouldNotBeNull();
        holdPeriodDays.Value.ShouldBeInRange(1, 29);
    }

    [Fact]
    public void CreateRandom_ProducesOneWeightPerInputKind()
    {
        // Arrange & Act
        var chromosome = StrategyChromosome.CreateRandom();

        // Assert
        chromosome.InputWeights.Select(w => w.Kind).ShouldBe(InputKinds, ignoreOrder: false);
        chromosome.InputWeights.Count.ShouldBe(InputKinds.Length);
    }

    [Fact]
    public void Mutate_WhenRateIsZero_ShouldNotChangeConfiguration()
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom();
        var originalConfig = chromosome.Configuration;

        // Act
        chromosome.Mutate(0.0);

        // Assert
        chromosome.Configuration.ShouldBe(originalConfig);
    }

    [Fact]
    public void Mutate_WhenRateIsZero_NeverChangesGenes()
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom();

        // Act
        var before = chromosome.Genes;
        for (var i = 0; i < 25; i++)
        {
            chromosome.Mutate(0.0);
        }

        var after = chromosome.Genes;

        // Assert
        after.ShouldBe(before);
    }

    [Fact]
    public void Mutate_WhenRateIsOne_ChangesAtMostOneFeatureGroupPerCall()
    {
        // Arrange
        var chromosome = ChromosomeWithAllCommonFields();
        var ranges = new (int min, int max)[] { (0, 3), (3, 10), (10, 12) };

        // Act & Assert
        for (var i = 0; i < 50; i++)
        {
            var before = chromosome.Genes;
            chromosome.Mutate(1.0);
            var after = chromosome.Genes;

            var changedGroups = ranges.Count(r =>
                Enumerable.Range(r.min, r.max - r.min).Any(g => before[g] != after[g])
            );
            changedGroups.ShouldBeLessThanOrEqualTo(1);
        }
    }

    [Fact]
    public void Crossover_WhenPartnerIsChromosome_ShouldReturnChromosomeChild()
    {
        // Arrange
        var parent1 = StrategyChromosome.CreateRandom();
        var parent2 = StrategyChromosome.CreateRandom();

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        child.ShouldNotBeNull();
        child.ShouldBeOfType<StrategyChromosome>();
        ((StrategyChromosome)child).Configuration.ShouldNotBeNull();
    }

    [Fact]
    public void Crossover_ChildGenesAllComeFromEitherParent()
    {
        // Arrange
        var parent1 = StrategyChromosome.CreateRandom();
        var parent2 = StrategyChromosome.CreateRandom();
        var parent1Genes = parent1.Genes;
        var parent2Genes = parent2.Genes;

        // Act
        var child = (StrategyChromosome)parent1.Crossover(parent2);
        var childGenes = child.Genes;

        // Assert
        childGenes
            .Select((g, i) => (g, i))
            .ShouldAllBe(t => t.g == parent1Genes[t.i] || t.g == parent2Genes[t.i]);
    }

    [Fact]
    public void Crossover_WhenPartnerIsNotChromosome_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var parent = StrategyChromosome.CreateRandom();
        var other = Substitute.For<IChromosome<double>>();

        // Act
        var crossover = () => parent.Crossover(other);

        // Assert
        crossover.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void ApplyConfigOverrides_SetsBothInputWeightsAndThresholdsOverrides()
    {
        // Arrange
        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            new InputThresholds(BuyThreshold: 0.1m)
        );

        var parameters = new BacktestParameters(
            new Id<Market>(Guid.NewGuid().ToString()),
            strategy,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(10),
            10000m
        );

        var chromosome = new StrategyChromosome(
            new TradingConfiguration { MaxPositions = 3 },
            [new(InputKind.SignalRsi, 2m)],
            new InputThresholds(SellThreshold: -0.2m)
        );

        // Act
        var overridden = chromosome.ApplyConfigOverrides(parameters);

        // Assert
        overridden.InputWeightsOverride.ShouldNotBeNull();
        overridden.InputWeightsOverride.ShouldBe(chromosome.InputWeights, ignoreOrder: false);
        overridden.ThresholdsOverride.ShouldBe(chromosome.Thresholds);
    }
}
