using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization;

public sealed class StrategyConfigurationChromosomeTests
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
    public void Constructor_WhenGivenTypeOnly_ShouldCreateRandomConfiguration(StrategyType type)
    {
        // Arrange & Act
        var chromosome = new StrategyConfigurationChromosome(type);

        // Assert
        chromosome.Configuration.ShouldNotBeNull();
        chromosome.Configuration.ShouldBeAssignableTo<StrategyConfiguration>();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Constructor_WhenGivenExplicitConfig_ShouldStoreIt(StrategyType type)
    {
        // Arrange
        var config = new StrategyConfiguration(MaxPositions: 15, HoldPeriodDays: 10);

        // Act
        var chromosome = new StrategyConfigurationChromosome(type, config);

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
        var chromosome = new StrategyConfigurationChromosome(type);

        // Act
        var asInterface = chromosome as IChromosome<double>;

        // Assert
        asInterface.ShouldNotBeNull();
        asInterface.Genes.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WhenGivenTypeOnly_ShouldProduceVariedConfigsAcrossCalls()
    {
        // Arrange
        var configs = new List<StrategyConfiguration>();

        // Act
        for (var i = 0; i < 10; i++)
        {
            var chromosome = new StrategyConfigurationChromosome(StrategyType.SignalWeighted);
            configs.Add(chromosome.Configuration);
        }

        // Assert
        var distinctMaxPositions = configs.Select(c => c.MaxPositions).Distinct().Count();
        distinctMaxPositions.ShouldBeGreaterThan(1);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void CreateRandomConfiguration_WhenAnyType_ShouldSetCommonFields(StrategyType type)
    {
        // Arrange & Act
        var config = StrategyConfigurationChromosome.CreateRandomConfiguration(type);

        // Assert
        config.MaxPositions.ShouldNotBeNull();
        config.MaxPositions.Value.ShouldBeInRange(1, 19);
        config.MaxPositionPercent.ShouldNotBeNull();
        config.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);
        config.HoldPeriodDays.ShouldNotBeNull();
        config.HoldPeriodDays.Value.ShouldBeInRange(1, 29);
    }

    [Fact]
    public void CreateRandomConfiguration_WhenSignalWeighted_ShouldSetSignalWeightedFields()
    {
        // Arrange & Act
        var config = StrategyConfigurationChromosome.CreateRandomConfiguration(StrategyType.SignalWeighted);

        // Assert
        config.SignalWeights.ShouldNotBeNull();
        config.SignalWeights.Count.ShouldBeGreaterThan(0);
        foreach (var sw in config.SignalWeights)
        {
            sw.Weight.ShouldBeInRange(0m, 2m);
        }

        config.BuyThreshold.ShouldNotBeNull();
        config.BuyThreshold.Value.ShouldBeInRange(0m, 1m);

        config.SellThreshold.ShouldNotBeNull();
        config.SellThreshold.Value.ShouldBeLessThanOrEqualTo(0m);

        config.ForecastMovementThreshold.ShouldBeNull();
        config.ForecastHorizonDays.ShouldBeNull();
    }

    [Fact]
    public void CreateRandomConfiguration_WhenSignalWeighted_ShouldIncludeAllSignalTypes()
    {
        // Arrange & Act
        var config = StrategyConfigurationChromosome.CreateRandomConfiguration(StrategyType.SignalWeighted);

        // Assert
        config.SignalWeights.ShouldNotBeNull();
        var signalTypes = config.SignalWeights.Select(sw => sw.Type).OrderBy(t => t).ToList();
        var allSignalTypes = Enum.GetValues<SignalType>().OrderBy(t => t).ToList();
        signalTypes.ShouldBe(allSignalTypes);
    }

    [Fact]
    public void CreateRandomConfiguration_WhenForecastMomentum_ShouldSetForecastMomentumFields()
    {
        // Arrange & Act
        var config = StrategyConfigurationChromosome.CreateRandomConfiguration(StrategyType.ForecastMomentum);

        // Assert
        config.ForecastMovementThreshold.ShouldNotBeNull();
        config.ForecastMovementThreshold.Value.ShouldBeInRange(0.01m, 0.11m);

        config.ForecastHorizonDays.ShouldNotBeNull();
        config.ForecastHorizonDays.Value.ShouldBeInRange(1, 13);

        config.BuyThreshold.ShouldBeNull();
        config.SellThreshold.ShouldBeNull();
        config.SignalWeights.ShouldBeNull();
        config.DeviationMultiplier.ShouldBeNull();
        config.MeanTimeFrameValue.ShouldBeNull();
        config.MinMarginPercent.ShouldBeNull();
    }

    [Fact]
    public void CreateRandomConfiguration_WhenMeanReversion_ShouldSetMeanReversionFields()
    {
        // Arrange & Act
        var config = StrategyConfigurationChromosome.CreateRandomConfiguration(StrategyType.MeanReversion);

        // Assert
        config.DeviationMultiplier.ShouldNotBeNull();
        config.DeviationMultiplier.Value.ShouldBeInRange(0.5m, 3.5m);

        config.MeanTimeFrameValue.ShouldNotBeNull();
        config.MeanTimeFrameValue.Value.ShouldBeInRange(1, 3);

        config.BuyThreshold.ShouldBeNull();
        config.SellThreshold.ShouldBeNull();
        config.ForecastMovementThreshold.ShouldBeNull();
        config.ForecastHorizonDays.ShouldBeNull();
        config.MinMarginPercent.ShouldBeNull();
    }

    [Fact]
    public void CreateRandomConfiguration_WhenRecipeArbitrage_ShouldSetRecipeArbitrageFields()
    {
        // Arrange & Act
        var config = StrategyConfigurationChromosome.CreateRandomConfiguration(StrategyType.RecipeArbitrage);

        // Assert
        config.MinMarginPercent.ShouldNotBeNull();
        config.MinMarginPercent.Value.ShouldBeInRange(0.01m, 0.21m);

        config.BuyThreshold.ShouldBeNull();
        config.SellThreshold.ShouldBeNull();
        config.ForecastMovementThreshold.ShouldBeNull();
        config.ForecastHorizonDays.ShouldBeNull();
        config.DeviationMultiplier.ShouldBeNull();
        config.MeanTimeFrameValue.ShouldBeNull();
    }

    [Fact]
    public void CreateRandomConfiguration_WhenComposite_ShouldOnlySetCommonFields()
    {
        // Arrange & Act
        var config = StrategyConfigurationChromosome.CreateRandomConfiguration(StrategyType.Composite);

        // Assert
        config.MaxPositions.ShouldNotBeNull();
        config.MaxPositions.Value.ShouldBeInRange(1, 19);
        config.MaxPositionPercent.ShouldNotBeNull();
        config.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);
        config.HoldPeriodDays.ShouldNotBeNull();
        config.HoldPeriodDays.Value.ShouldBeInRange(1, 29);

        config.BuyThreshold.ShouldBeNull();
        config.SellThreshold.ShouldBeNull();
        config.SignalWeights.ShouldBeNull();
        config.ForecastMovementThreshold.ShouldBeNull();
        config.ForecastHorizonDays.ShouldBeNull();
        config.DeviationMultiplier.ShouldBeNull();
        config.MeanTimeFrameValue.ShouldBeNull();
        config.MinMarginPercent.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Genes_WhenCalled_ShouldReturnNonEmptyArray(StrategyType type)
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(type);

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
        var chromosome = new StrategyConfigurationChromosome(type);
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

    [Fact]
    public void Genes_WhenSignalWeighted_ShouldIncludeSignalWeightedSpecificGenes()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.SignalWeighted);
        var config = chromosome.Configuration;
        var genes = chromosome.Genes;

        // Act & Assert
        var expectedCount = 3 + 2 + config.SignalWeights!.Count;
        genes.Length.ShouldBe(expectedCount);
    }

    [Fact]
    public void Genes_WhenForecastMomentum_ShouldIncludeForecastMomentumSpecificGenes()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.ForecastMomentum);

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(5);
    }

    [Fact]
    public void Genes_WhenMeanReversion_ShouldIncludeMeanReversionSpecificGenes()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.MeanReversion);

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(5);
    }

    [Fact]
    public void Genes_WhenRecipeArbitrage_ShouldIncludeRecipeArbitrageSpecificGenes()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.RecipeArbitrage);

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(4);
    }

    [Fact]
    public void Genes_WhenComposite_ShouldOnlyReturnCommonGenes()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.Composite);

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.ShouldNotBeNull();
        genes.Length.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Genes_WhenSignalWeightedAndSignalWeightsNull_ShouldNotThrow()
    {
        // Arrange
        var config = new StrategyConfiguration(
            MaxPositions: 5,
            MaxPositionPercent: 0.1m,
            HoldPeriodDays: 10
        );

        // Act
        var chromosome = new StrategyConfigurationChromosome(StrategyType.SignalWeighted, config);

        // Assert
        var genes = chromosome.Genes;
        genes.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Mutate_WhenRateIsZero_ShouldNotChangeConfiguration(StrategyType type)
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(type);
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
        var chromosome = new StrategyConfigurationChromosome(type);

        // Act & Assert
        Should.NotThrow(() => chromosome.Mutate(0.5));
    }

    [Fact]
    public void Mutate_WhenMaxPositionsHasValue_CanChangeIt()
    {
        // Arrange
        var config = new StrategyConfiguration(
            MaxPositions: 10,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 15
        );
        var chromosome = new StrategyConfigurationChromosome(StrategyType.Composite, config);

        // Act
        for (var i = 0; i < 100; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Mutate_WhenMaxPositionPercentHasValue_ShouldClampWithinBounds()
    {
        // Arrange
        var config = new StrategyConfiguration(
            MaxPositions: 10,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 15
        );
        var chromosome = new StrategyConfigurationChromosome(StrategyType.Composite, config);

        // Act
        for (var i = 0; i < 200; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        chromosome.Configuration.MaxPositionPercent.ShouldNotBeNull();
        chromosome.Configuration.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.5m);
    }

    [Fact]
    public void Mutate_WhenHoldPeriodDaysHasValue_ShouldStayAtLeastOne()
    {
        // Arrange
        var config = new StrategyConfiguration(
            MaxPositions: 10,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 2
        );
        var chromosome = new StrategyConfigurationChromosome(StrategyType.Composite, config);

        // Act
        for (var i = 0; i < 200; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        chromosome.Configuration.HoldPeriodDays.ShouldNotBeNull();
        chromosome.Configuration.HoldPeriodDays.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Mutate_WhenSignalWeightedType_ShouldMutateSignalSpecificFields()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.SignalWeighted);

        // Act
        for (var i = 0; i < 200; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        var config = chromosome.Configuration;
        if (config.BuyThreshold.HasValue)
        {
            config.BuyThreshold.Value.ShouldBeInRange(0.01m, 2m);
        }

        if (config.SellThreshold.HasValue)
        {
            config.SellThreshold.Value.ShouldBeLessThanOrEqualTo(-0.01m);
            config.SellThreshold.Value.ShouldBeGreaterThanOrEqualTo(-2m);
        }

        if (config.ForecastMovementThreshold.HasValue)
        {
            config.ForecastMovementThreshold.Value.ShouldBeInRange(0.005m, 0.2m);
        }

        if (config.ForecastHorizonDays.HasValue)
        {
            config.ForecastHorizonDays.Value.ShouldBeInRange(1, 30);
        }
    }

    [Fact]
    public void Mutate_WhenForecastMomentumType_ShouldMutateForecastMomentumFields()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.ForecastMomentum);

        // Act
        for (var i = 0; i < 200; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        var config = chromosome.Configuration;
        if (config.ForecastMovementThreshold.HasValue)
        {
            config.ForecastMovementThreshold.Value.ShouldBeInRange(0.005m, 0.2m);
        }

        if (config.ForecastHorizonDays.HasValue)
        {
            config.ForecastHorizonDays.Value.ShouldBeInRange(1, 30);
        }
    }

    [Fact]
    public void Mutate_WhenMeanReversionType_ShouldMutateMeanReversionFields()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.MeanReversion);

        // Act
        for (var i = 0; i < 200; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        var config = chromosome.Configuration;
        if (config.DeviationMultiplier.HasValue)
        {
            config.DeviationMultiplier.Value.ShouldBeInRange(0.5m, 4m);
        }

        if (config.MeanTimeFrameValue.HasValue)
        {
            config.MeanTimeFrameValue.Value.ShouldBeInRange(1, 4);
        }
    }

    [Fact]
    public void Mutate_WhenRecipeArbitrageType_ShouldMutateMinMarginPercent()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(StrategyType.RecipeArbitrage);

        // Act
        for (var i = 0; i < 200; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        var config = chromosome.Configuration;
        if (config.MinMarginPercent.HasValue)
        {
            config.MinMarginPercent.Value.ShouldBeInRange(0.005m, 0.3m);
        }
    }

    [Fact]
    public void Mutate_WhenCompositeType_ShouldOnlyMutateCommonFields()
    {
        // Arrange
        var config = new StrategyConfiguration(
            MaxPositions: 10,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 15
        );
        var chromosome = new StrategyConfigurationChromosome(StrategyType.Composite, config);

        // Act
        chromosome.Mutate(1.0);

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Crossover_WhenBothSameType_ShouldReturnChildOfSameType(StrategyType type)
    {
        // Arrange
        var parent1 = new StrategyConfigurationChromosome(type);
        var parent2 = new StrategyConfigurationChromosome(type);

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        child.ShouldNotBeNull();
        child.ShouldBeOfType<StrategyConfigurationChromosome>();
        var childChromosome = (StrategyConfigurationChromosome)child;
        childChromosome.Configuration.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Crossover_WhenBothSameType_ChildShouldInheritFieldsFromParents(StrategyType type)
    {
        // Arrange
        var config1 = new StrategyConfiguration(
            MaxPositions: 3,
            MaxPositionPercent: 0.10m,
            HoldPeriodDays: 7
        );
        var config2 = new StrategyConfiguration(
            MaxPositions: 15,
            MaxPositionPercent: 0.40m,
            HoldPeriodDays: 25
        );
        var parent1 = new StrategyConfigurationChromosome(type, config1);
        var parent2 = new StrategyConfigurationChromosome(type, config2);

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        var childConfig = ((StrategyConfigurationChromosome)child).Configuration;

        int?[] validMaxPositions = [config1.MaxPositions, config2.MaxPositions];
        validMaxPositions.ShouldContain(childConfig.MaxPositions);

        var validMaxPositionPercents = new[] { config1.MaxPositionPercent, config2.MaxPositionPercent };
        validMaxPositionPercents.ShouldContain(childConfig.MaxPositionPercent);

        int?[] validHoldPeriodDays = [config1.HoldPeriodDays, config2.HoldPeriodDays];
        validHoldPeriodDays.ShouldContain(childConfig.HoldPeriodDays);
    }

    [Fact]
    public void Crossover_WhenSignalWeighted_ChildShouldInheritSignalWeightedFields()
    {
        // Arrange
        var sw1 = new List<SignalWeight> { new(SignalType.Rsi, 1.0m), new(SignalType.TrendMomentum, 2.0m), };
        var sw2 = new List<SignalWeight> { new(SignalType.Rsi, 0.5m), new(SignalType.TrendMomentum, 1.5m), };
        var config1 = new StrategyConfiguration(
            SignalWeights: sw1,
            BuyThreshold: 0.5m,
            SellThreshold: -0.5m,
            MaxPositions: 5,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 10,
            ForecastMovementThreshold: 0.05m,
            ForecastHorizonDays: 7
        );
        var config2 = new StrategyConfiguration(
            SignalWeights: sw2,
            BuyThreshold: 1.5m,
            SellThreshold: -1.0m,
            MaxPositions: 15,
            MaxPositionPercent: 0.4m,
            HoldPeriodDays: 20,
            ForecastMovementThreshold: 0.10m,
            ForecastHorizonDays: 3
        );
        var parent1 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted, config1);
        var parent2 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted, config2);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);
        var childConfig = child.Configuration;

        // Assert
        var validBuyThresholds = new[] { config1.BuyThreshold, config2.BuyThreshold };
        validBuyThresholds.ShouldContain(childConfig.BuyThreshold);

        decimal?[] validSellThresholds = [config1.SellThreshold, config2.SellThreshold];
        validSellThresholds.ShouldContain(childConfig.SellThreshold);

        decimal?[] validMovementThresholds = [config1.ForecastMovementThreshold, config2.ForecastMovementThreshold];
        validMovementThresholds.ShouldContain(childConfig.ForecastMovementThreshold);

        var validHorizonDays = new[] { config1.ForecastHorizonDays, config2.ForecastHorizonDays };
        validHorizonDays.ShouldContain(childConfig.ForecastHorizonDays);

        childConfig.SignalWeights.ShouldNotBeNull();
        foreach (var sw in childConfig.SignalWeights)
        {
            var p1Weight = sw1.First(s => s.Type == sw.Type).Weight;
            var p2Weight = sw2.First(s => s.Type == sw.Type).Weight;
            var validWeights = new[] { p1Weight, p2Weight };
            validWeights.ShouldContain(sw.Weight);
        }
    }

    [Fact]
    public void Crossover_WhenForecastMomentum_ChildShouldInheritForecastMomentumFields()
    {
        // Arrange
        var config1 = new StrategyConfiguration(
            MaxPositions: 5,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 10,
            ForecastMovementThreshold: 0.05m,
            ForecastHorizonDays: 7
        );
        var config2 = new StrategyConfiguration(
            MaxPositions: 15,
            MaxPositionPercent: 0.4m,
            HoldPeriodDays: 20,
            ForecastMovementThreshold: 0.10m,
            ForecastHorizonDays: 3
        );
        var parent1 = new StrategyConfigurationChromosome(StrategyType.ForecastMomentum, config1);
        var parent2 = new StrategyConfigurationChromosome(StrategyType.ForecastMomentum, config2);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);
        var childConfig = child.Configuration;

        // Assert
        var validMovementThresholds =
            new[] { config1.ForecastMovementThreshold, config2.ForecastMovementThreshold };
        validMovementThresholds.ShouldContain(childConfig.ForecastMovementThreshold);

        var validHorizonDays = new[] { config1.ForecastHorizonDays, config2.ForecastHorizonDays };
        validHorizonDays.ShouldContain(childConfig.ForecastHorizonDays);
    }

    [Fact]
    public void Crossover_WhenMeanReversion_ChildShouldInheritMeanReversionFields()
    {
        // Arrange
        var config1 = new StrategyConfiguration(
            MaxPositions: 5,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 10,
            DeviationMultiplier: 1.0m,
            MeanTimeFrameValue: 2
        );
        var config2 = new StrategyConfiguration(
            MaxPositions: 15,
            MaxPositionPercent: 0.4m,
            HoldPeriodDays: 20,
            DeviationMultiplier: 3.0m,
            MeanTimeFrameValue: 4
        );
        var parent1 = new StrategyConfigurationChromosome(StrategyType.MeanReversion, config1);
        var parent2 = new StrategyConfigurationChromosome(StrategyType.MeanReversion, config2);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);
        var childConfig = child.Configuration;

        // Assert
        var validDeviationMultipliers = new[] { config1.DeviationMultiplier, config2.DeviationMultiplier };
        validDeviationMultipliers.ShouldContain(childConfig.DeviationMultiplier);

        int?[] validMeanTimeFrameValues = [config1.MeanTimeFrameValue, config2.MeanTimeFrameValue];
        validMeanTimeFrameValues.ShouldContain(childConfig.MeanTimeFrameValue);
    }

    [Fact]
    public void Crossover_WhenRecipeArbitrage_ChildShouldInheritMinMarginPercent()
    {
        // Arrange
        var config1 = new StrategyConfiguration(
            MaxPositions: 5,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 10,
            MinMarginPercent: 0.05m
        );
        var config2 = new StrategyConfiguration(
            MaxPositions: 15,
            MaxPositionPercent: 0.4m,
            HoldPeriodDays: 20,
            MinMarginPercent: 0.15m
        );
        var parent1 = new StrategyConfigurationChromosome(StrategyType.RecipeArbitrage, config1);
        var parent2 = new StrategyConfigurationChromosome(StrategyType.RecipeArbitrage, config2);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);
        var childConfig = child.Configuration;

        // Assert
        var validMinMargins = new[] { config1.MinMarginPercent, config2.MinMarginPercent };
        validMinMargins.ShouldContain(childConfig.MinMarginPercent);
    }

    [Fact]
    public void Crossover_WhenCompositeType_ShouldOnlyCrossoverCommonFields()
    {
        // Arrange
        var config1 = new StrategyConfiguration(MaxPositions: 3, MaxPositionPercent: 0.10m, HoldPeriodDays: 7);
        var config2 = new StrategyConfiguration(MaxPositions: 15, MaxPositionPercent: 0.40m, HoldPeriodDays: 25);
        var parent1 = new StrategyConfigurationChromosome(StrategyType.Composite, config1);
        var parent2 = new StrategyConfigurationChromosome(StrategyType.Composite, config2);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);
        var childConfig = child.Configuration;

        // Assert
        int?[] validMaxPositions = [config1.MaxPositions, config2.MaxPositions];
        validMaxPositions.ShouldContain(childConfig.MaxPositions);

        childConfig.BuyThreshold.ShouldBeNull();
        childConfig.SellThreshold.ShouldBeNull();
        childConfig.SignalWeights.ShouldBeNull();
        childConfig.ForecastMovementThreshold.ShouldBeNull();
        childConfig.ForecastHorizonDays.ShouldBeNull();
        childConfig.DeviationMultiplier.ShouldBeNull();
        childConfig.MeanTimeFrameValue.ShouldBeNull();
        childConfig.MinMarginPercent.ShouldBeNull();
    }

    [Fact]
    public void Crossover_WhenDifferentTypes_ShouldSucceedWithFirstParentsType()
    {
        // Arrange
        var parent1 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted);
        var other = new StrategyConfigurationChromosome(StrategyType.ForecastMomentum);

        // Act
        var child = parent1.Crossover(other);

        // Assert
        child.ShouldNotBeNull();
        child.ShouldBeOfType<StrategyConfigurationChromosome>();
    }

    [Fact]
    public void Crossover_WhenOtherIsNotStrategyConfigurationChromosome_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var parent1 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted);
        var other = Substitute.For<IChromosome<double>>();

        // Act
        var crossover = () => parent1.Crossover(other);

        // Assert
        crossover.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Crossover_WhenBothParents_ShouldProduceDeterministicStructure()
    {
        // Arrange
        var config1 = new StrategyConfiguration(
            MaxPositions: 5,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 10,
            BuyThreshold: 0.5m,
            SellThreshold: -0.5m,
            SignalWeights: [new(SignalType.Rsi, 1.0m), new(SignalType.TrendMomentum, 2.0m),],
            ForecastMovementThreshold: 0.05m,
            ForecastHorizonDays: 7
        );
        var config2 = new StrategyConfiguration(
            MaxPositions: 15,
            MaxPositionPercent: 0.4m,
            HoldPeriodDays: 20,
            BuyThreshold: 1.5m,
            SellThreshold: -1.0m,
            SignalWeights: [new(SignalType.Rsi, 0.5m), new(SignalType.TrendMomentum, 1.5m),],
            ForecastMovementThreshold: 0.10m,
            ForecastHorizonDays: 3
        );
        var parent1 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted, config1);
        var parent2 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted, config2);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);

        // Assert
        child.Configuration.SignalWeights.ShouldNotBeNull();
        child.Configuration.SignalWeights.Count.ShouldBe(2);
    }

    [Fact]
    public void Crossover_WhenOneParentHasNullSignalWeights_ShouldUseNonNullParentWeights()
    {
        // Arrange
        var config1 = new StrategyConfiguration(
            MaxPositions: 5,
            MaxPositionPercent: 0.2m,
            HoldPeriodDays: 10,
            BuyThreshold: 0.5m,
            SellThreshold: -0.5m,
            SignalWeights: null,
            ForecastMovementThreshold: 0.05m,
            ForecastHorizonDays: 7
        );
        var config2 = new StrategyConfiguration(
            MaxPositions: 15,
            MaxPositionPercent: 0.4m,
            HoldPeriodDays: 20,
            BuyThreshold: 1.5m,
            SellThreshold: -1.0m,
            SignalWeights: [new(SignalType.Rsi, 0.5m)],
            ForecastMovementThreshold: 0.10m,
            ForecastHorizonDays: 3
        );
        var parent1 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted, config1);
        var parent2 = new StrategyConfigurationChromosome(StrategyType.SignalWeighted, config2);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);

        // Assert
        child.Configuration.SignalWeights.ShouldNotBeNull();
        child.Configuration.SignalWeights.Count.ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Interface_Genes_ShouldReturnDoubleArray(StrategyType type)
    {
        // Arrange
        IChromosome<double> chromosome = new StrategyConfigurationChromosome(type);

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
        IChromosome<double> parent1 = new StrategyConfigurationChromosome(type);
        IChromosome<double> parent2 = new StrategyConfigurationChromosome(type);

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        child.ShouldBeAssignableTo<IChromosome<double>>();
    }

    [Theory]
    [MemberData(nameof(StrategyTypeData))]
    public void Crossover_Result_ShouldHaveSameTypeAsParents(StrategyType type)
    {
        // Arrange
        var parent1 = new StrategyConfigurationChromosome(type);
        var parent2 = new StrategyConfigurationChromosome(type);

        // Act
        var child = (StrategyConfigurationChromosome)parent1.Crossover(parent2);

        // Assert
        var childGenes = child.Genes;
        childGenes.Length.ShouldBeGreaterThan(0);
    }
}
