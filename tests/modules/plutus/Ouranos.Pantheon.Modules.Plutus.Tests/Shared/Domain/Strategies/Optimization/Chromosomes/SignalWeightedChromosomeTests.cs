using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class SignalWeightedChromosomeTests
{
    [Fact]
    public void CreateRandom_WhenSignalWeighted_ShouldSetSignalWeightedFields()
    {
        // Arrange & Act
        var chromosome = (SignalWeightedChromosome)StrategyChromosome.CreateRandom(StrategyType.SignalWeighted);

        // Assert
        chromosome.SignalWeightedConfig.TaxAdjustedRoiWeight.ShouldNotBeNull();
        chromosome.SignalWeightedConfig.TaxAdjustedRoiWeight.Value.ShouldBeInRange(0m, 2m);
        chromosome.SignalWeightedConfig.VolumeAnomalyWeight.ShouldNotBeNull();
        chromosome.SignalWeightedConfig.VolumeAnomalyWeight.Value.ShouldBeInRange(0m, 2m);
        chromosome.SignalWeightedConfig.TrendMomentumWeight.ShouldNotBeNull();
        chromosome.SignalWeightedConfig.TrendMomentumWeight.Value.ShouldBeInRange(0m, 2m);
        chromosome.SignalWeightedConfig.BollingerBandsWeight.ShouldNotBeNull();
        chromosome.SignalWeightedConfig.BollingerBandsWeight.Value.ShouldBeInRange(0m, 2m);
        chromosome.SignalWeightedConfig.RsiWeight.ShouldNotBeNull();
        chromosome.SignalWeightedConfig.RsiWeight.Value.ShouldBeInRange(0m, 2m);
        chromosome.SignalWeightedConfig.MovingAverageCrossoverWeight.ShouldNotBeNull();
        chromosome.SignalWeightedConfig.MovingAverageCrossoverWeight.Value.ShouldBeInRange(0m, 2m);
        chromosome.SignalWeightedConfig.PriceVelocityWeight.ShouldNotBeNull();
        chromosome.SignalWeightedConfig.PriceVelocityWeight.Value.ShouldBeInRange(0m, 2m);
    }

    [Fact]
    public void Genes_WhenSignalWeighted_ShouldIncludeSignalWeightedSpecificGenes()
    {
        // Arrange
        var chromosome = StrategyChromosome.CreateRandom(StrategyType.SignalWeighted);
        var genes = chromosome.Genes;

        // Act & Assert
        var expectedCount = 3 + 7;
        genes.Length.ShouldBe(expectedCount);
    }

    [Fact]
    public void Genes_WhenSignalWeightedAndSignalWeightedConfigHasNoWeightsSet_ShouldNotThrow()
    {
        // Arrange
        var config = new TradingConfiguration { MaxPositions = 5, MaxPositionPercent = 0.1m, HoldPeriodDays = 10 };

        // Act
        var chromosome = new SignalWeightedChromosome(config);

        // Assert
        var genes = chromosome.Genes;
        genes.ShouldNotBeNull();
        genes.Length.ShouldBe(10);
    }

    [Fact]
    public void Mutate_WhenSignalWeightedType_ShouldMutateSignalWeightedConfig()
    {
        // Arrange
        var chromosome = (SignalWeightedChromosome)StrategyChromosome.CreateRandom(StrategyType.SignalWeighted);

        // Act
        for (var i = 0; i < 200; i++)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        var config = chromosome.Configuration;
        config.MaxPositions.ShouldNotBeNull();
        config.MaxPositionPercent.ShouldNotBeNull();
        config.HoldPeriodDays.ShouldNotBeNull();
    }

    [Fact]
    public void Mutate_WhenSignalWeighted_ShouldNotThrow()
    {
        // Arrange
        var chromosome = (SignalWeightedChromosome)StrategyChromosome.CreateRandom(StrategyType.SignalWeighted);

        // Act & Assert
        Should.NotThrow(() => chromosome.Mutate(0.5));
    }

    [Fact]
    public void Crossover_WhenSignalWeighted_ChildShouldInheritSignalWeightedFields()
    {
        // Arrange
        var config1 = new TradingConfiguration { MaxPositions = 5, MaxPositionPercent = 0.2m, HoldPeriodDays = 10, };
        var weights1 = new SignalWeightedConfig(
            TaxAdjustedRoiWeight: 1.0m,
            VolumeAnomalyWeight: 2.0m,
            TrendMomentumWeight: 0.5m,
            BollingerBandsWeight: 1.5m,
            RsiWeight: 1.0m,
            MovingAverageCrossoverWeight: 2.0m,
            PriceVelocityWeight: 0.8m
        );
        var config2 = new TradingConfiguration { MaxPositions = 15, MaxPositionPercent = 0.4m, HoldPeriodDays = 20, };
        var weights2 = new SignalWeightedConfig(
            TaxAdjustedRoiWeight: 0.5m,
            VolumeAnomalyWeight: 1.5m,
            TrendMomentumWeight: 1.0m,
            BollingerBandsWeight: 2.0m,
            RsiWeight: 0.5m,
            MovingAverageCrossoverWeight: 1.5m,
            PriceVelocityWeight: 1.2m
        );
        var parent1 = new SignalWeightedChromosome(config1, weights1);
        var parent2 = new SignalWeightedChromosome(config2, weights2);

        // Act
        var child = (SignalWeightedChromosome)parent1.Crossover(parent2);
        var childConfig = child.Configuration;

        // Assert
        var validMaxPositions = new[] { config1.MaxPositions, config2.MaxPositions };
        validMaxPositions.ShouldContain(childConfig.MaxPositions);

        var validMaxPositionPercents = new[] { config1.MaxPositionPercent, config2.MaxPositionPercent };
        validMaxPositionPercents.ShouldContain(childConfig.MaxPositionPercent);

        var validTaxAdjustedRoi = new[] { weights1.TaxAdjustedRoiWeight, weights2.TaxAdjustedRoiWeight };
        validTaxAdjustedRoi.ShouldContain(child.SignalWeightedConfig.TaxAdjustedRoiWeight);

        var validVolumeAnomaly = new[] { weights1.VolumeAnomalyWeight, weights2.VolumeAnomalyWeight };
        validVolumeAnomaly.ShouldContain(child.SignalWeightedConfig.VolumeAnomalyWeight);

        var validRsi = new[] { weights1.RsiWeight, weights2.RsiWeight };
        validRsi.ShouldContain(child.SignalWeightedConfig.RsiWeight);
    }

    [Fact]
    public void Crossover_WhenBothParents_ShouldProduceDeterministicStructure()
    {
        // Arrange
        var config1 = new TradingConfiguration { MaxPositions = 5, MaxPositionPercent = 0.2m, HoldPeriodDays = 10, };
        var weights1 = new SignalWeightedConfig(RsiWeight: 1.0m, TrendMomentumWeight: 2.0m);
        var config2 = new TradingConfiguration { MaxPositions = 15, MaxPositionPercent = 0.4m, HoldPeriodDays = 20, };
        var weights2 = new SignalWeightedConfig(RsiWeight: 0.5m, TrendMomentumWeight: 1.5m);
        var parent1 = new SignalWeightedChromosome(config1, weights1);
        var parent2 = new SignalWeightedChromosome(config2, weights2);

        // Act
        var child = (SignalWeightedChromosome)parent1.Crossover(parent2);

        // Assert
        child.SignalWeightedConfig.RsiWeight.ShouldNotBeNull();
        child.SignalWeightedConfig.TrendMomentumWeight.ShouldNotBeNull();
    }

    [Fact]
    public void ApplyConfigOverrides_WhenSignalWeighted_ShouldSetSignalWeightedOverride()
    {
        // Arrange
        var config = new TradingConfiguration();
        var weights = new SignalWeightedConfig(
            BuyThreshold: 50m,
            SellThreshold: 50m,
            TaxAdjustedRoiWeight: 1.0m,
            VolumeAnomalyWeight: 0.5m,
            TrendMomentumWeight: 0.8m,
            BollingerBandsWeight: 1.2m,
            RsiWeight: 0.6m,
            MovingAverageCrossoverWeight: 0.9m,
            PriceVelocityWeight: 0.7m
        );
        var chromosome = new SignalWeightedChromosome(config, weights);
        var strategy = Strategy.Create(
            DatabaseExtensions.CreateId<Market>(),
            "Test",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var parameters = new BacktestParameters(
            DatabaseExtensions.CreateId<Market>(),
            strategy,
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
            10000m
        );

        // Act
        var result = chromosome.ApplyConfigOverrides(parameters);

        // Assert
        result.SignalWeightedConfigOverride.ShouldNotBeNull();
        result.SignalWeightedConfigOverride.ShouldBeSameAs(weights);
        result.ForecastMomentumConfigOverride.ShouldBeNull();
        result.MeanReversionConfigOverride.ShouldBeNull();
        result.RecipeArbitrageConfigOverride.ShouldBeNull();
    }
}
