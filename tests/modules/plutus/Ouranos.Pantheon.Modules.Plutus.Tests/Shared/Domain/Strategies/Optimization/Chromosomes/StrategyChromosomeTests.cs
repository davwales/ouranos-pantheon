using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class StrategyChromosomeTests
{
    private static readonly StrategyType[] AllStrategyTypes =
    [
        StrategyType.SignalWeighted,
        StrategyType.ForecastMomentum,
        StrategyType.MeanReversion,
        StrategyType.RecipeArbitrage,
        StrategyType.Composite,
    ];

    public static IEnumerable<object[]> StrategyTypeData =>
        AllStrategyTypes.Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void CreateRandom_WhenGivenTypeOnly_ShouldCreateRandomConfiguration(StrategyType type)
    {
        // Arrange & Act
        var chromosome = StrategyChromosome.CreateRandom(type);

        // Assert
        chromosome.ShouldNotBeNull();
        chromosome.Configuration.ShouldNotBeNull();
        chromosome.Configuration.ShouldBeAssignableTo<TradingConfiguration>();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Create_WhenGivenExplicitConfig_ShouldStoreIt(StrategyType type)
    {
        // Arrange
        var config = new TradingConfiguration { MaxPositions = 15, HoldPeriodDays = 10 };

        // Act
        var chromosome = StrategyChromosome.Create(type, config);

        // Assert
        chromosome.Configuration.ShouldBeSameAs(config);
        chromosome.Configuration.MaxPositions.ShouldBe(15);
        chromosome.Configuration.HoldPeriodDays.ShouldBe(10);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Implements_ShouldSatisfyIChromosomeContract(StrategyType type)
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom(type);

        // Act
        var asInterface = chromosome as IChromosome<double>;

        // Assert
        asInterface.ShouldNotBeNull();
        asInterface.Genes.ShouldNotBeNull();
    }

    [Fact]
    public void CreateRandom_WhenGivenTypeOnly_ShouldProduceVariedConfigsAcrossCalls()
    {
        // Arrange
        var configs = new List<TradingConfiguration>();

        // Act
        for (var i = 0; i < 10; i++)
        {
            var chromosome = StrategyChromosome.CreateRandom(StrategyType.SignalWeighted);
            configs.Add(chromosome.Configuration);
        }

        // Assert
        var distinctMaxPositions = configs.Select(c => c.MaxPositions).Distinct().Count();
        distinctMaxPositions.ShouldBeGreaterThan(1);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void CreateRandom_WhenAnyType_ShouldSetCommonFields(StrategyType type)
    {
        // Arrange & Act
        var chromosome = StrategyChromosome.CreateRandom(type);

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeInRange(1, 19);
        chromosome.Configuration.MaxPositionPercent.ShouldNotBeNull();
        chromosome.Configuration.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);
        chromosome.Configuration.HoldPeriodDays.ShouldNotBeNull();
        chromosome.Configuration.HoldPeriodDays.Value.ShouldBeInRange(1, 29);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Mutate_WhenRateIsZero_ShouldNotChangeConfiguration(StrategyType type)
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom(type);
        var originalConfig = chromosome.Configuration;

        // Act
        chromosome.Mutate(0.0);

        // Assert
        chromosome.Configuration.ShouldBe(originalConfig);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Mutate_WhenCalled_ShouldNotThrow(StrategyType type)
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom(type);

        // Act & Assert
        Should.NotThrow(() => chromosome.Mutate(0.5));
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Crossover_WhenBothSameType_ShouldReturnChildOfSameType(StrategyType type)
    {
        // Arrange
        var parent1 = StrategyChromosome.CreateRandom(type);
        var parent2 = StrategyChromosome.CreateRandom(type);

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        child.ShouldNotBeNull();
        child.ShouldBeAssignableTo<StrategyChromosome>();
        var childChromosome = (StrategyChromosome)child;
        childChromosome.Configuration.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Crossover_WhenBothSameType_ChildShouldInheritFieldsFromParents(StrategyType type)
    {
        // Arrange
        var config1 = new TradingConfiguration { MaxPositions = 3, MaxPositionPercent = 0.10m, HoldPeriodDays = 7 };
        var config2 = new TradingConfiguration { MaxPositions = 15, MaxPositionPercent = 0.40m, HoldPeriodDays = 25 };
        var parent1 = StrategyChromosome.Create(type, config1);
        var parent2 = StrategyChromosome.Create(type, config2);

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        var childConfig = ((StrategyChromosome)child).Configuration;

        int?[] validMaxPositions = [config1.MaxPositions, config2.MaxPositions];
        validMaxPositions.ShouldContain(childConfig.MaxPositions);

        var validMaxPositionPercents = new[] { config1.MaxPositionPercent, config2.MaxPositionPercent };
        validMaxPositionPercents.ShouldContain(childConfig.MaxPositionPercent);

        int?[] validHoldPeriodDays = [config1.HoldPeriodDays, config2.HoldPeriodDays];
        validHoldPeriodDays.ShouldContain(childConfig.HoldPeriodDays);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Crossover_Result_ShouldHaveSameTypeAsParents(StrategyType type)
    {
        // Arrange
        var parent1 = StrategyChromosome.CreateRandom(type);
        var parent2 = StrategyChromosome.CreateRandom(type);

        // Act
        var child = (StrategyChromosome)parent1.Crossover(parent2);

        // Assert
        var childGenes = child.Genes;
        childGenes.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Interface_Genes_ShouldReturnDoubleArray(StrategyType type)
    {
        // Arrange
        IChromosome<double> chromosome = StrategyChromosome.CreateRandom(type);

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.ShouldBeOfType<double[]>();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Interface_Crossover_ShouldReturnIChromosomeDouble(StrategyType type)
    {
        // Arrange
        IChromosome<double> parent1 = StrategyChromosome.CreateRandom(type);
        IChromosome<double> parent2 = StrategyChromosome.CreateRandom(type);

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        child.ShouldBeAssignableTo<IChromosome<double>>();
    }

    [Fact]
    public void Crossover_WhenDifferentTypes_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var parent1 = new SignalWeightedChromosome(new TradingConfiguration());
        var parent2 = new ForecastMomentumChromosome(new TradingConfiguration());

        // Act
        var crossover = () => parent1.Crossover(parent2);

        // Assert
        crossover.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Crossover_WhenOtherIsNotStrategyChromosome_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var parent1 = new SignalWeightedChromosome(new TradingConfiguration());
        var other = Substitute.For<IChromosome<double>>();

        // Act
        var crossover = () => parent1.Crossover(other);

        // Assert
        crossover.ShouldThrow<InvalidOperationException>();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Genes_WhenCalled_ShouldReturnNonEmptyArray(StrategyType type)
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom(type);

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.ShouldNotBeNull();
        genes.Length.ShouldBeGreaterThan(0);
        genes.ShouldAllBe(g => !double.IsNaN(g));
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Genes_ShouldBeConsistentWithConfiguration(StrategyType type)
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom(type);
        var config = chromosome.Configuration;
        var genes = chromosome.Genes;

        // Act & Assert
        var index = 0;

        if (config.MaxPositions.HasValue)
        {
            genes[index].ShouldBe(config.MaxPositions.Value);
            index++;
        }

        if (config.MaxPositionPercent.HasValue)
        {
            genes[index].ShouldBe((double)config.MaxPositionPercent.Value);
            index++;
        }

        if (config.HoldPeriodDays.HasValue)
        {
            genes[index].ShouldBe(config.HoldPeriodDays.Value);
        }
    }
}
