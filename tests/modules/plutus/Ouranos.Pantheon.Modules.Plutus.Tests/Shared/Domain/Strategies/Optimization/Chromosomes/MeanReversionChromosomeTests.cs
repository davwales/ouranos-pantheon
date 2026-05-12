using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class MeanReversionChromosomeTests
{
    [Fact]
    public void CreateRandom_WhenMeanReversion_ShouldSetCommonFieldsOnly()
    {
        // Arrange & Act
        var chromosome = (MeanReversionChromosome)
            StrategyChromosome.CreateRandom(StrategyType.MeanReversion);

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeInRange(1, 19);
        chromosome.Configuration.MaxPositionPercent.ShouldNotBeNull();
        chromosome.Configuration.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);
        chromosome.Configuration.HoldPeriodDays.ShouldNotBeNull();
        chromosome.Configuration.HoldPeriodDays.Value.ShouldBeInRange(1, 29);
    }

    [Fact]
    public void Genes_WhenMeanReversion_ShouldReturnCommonPlusTwoSpecific()
    {
        // Arrange
        var chromosome = new MeanReversionChromosome(
            new TradingConfiguration
            {
                MaxPositions = 5,
                MaxPositionPercent = 0.1m,
                HoldPeriodDays = 10,
            },
            new MeanReversionConfig(2.0m, 20)
        );

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(5);
    }

    [Fact]
    public void Mutate_WhenMeanReversion_ShouldNotThrow()
    {
        // Arrange
        var chromosome = new MeanReversionChromosome(
            new TradingConfiguration
            {
                MaxPositions = 10,
                MaxPositionPercent = 0.2m,
                HoldPeriodDays = 15,
            }
        );

        // Act & Assert
        Should.NotThrow(() => chromosome.Mutate(0.5));
    }

    [Fact]
    public void Crossover_WhenMeanReversion_ChildShouldInheritMeanReversionFields()
    {
        // Arrange
        var config1 = new TradingConfiguration
        {
            MaxPositions = 5,
            MaxPositionPercent = 0.2m,
            HoldPeriodDays = 10,
        };
        var mrConfig1 = new MeanReversionConfig(2.0m, 20);
        var config2 = new TradingConfiguration
        {
            MaxPositions = 15,
            MaxPositionPercent = 0.4m,
            HoldPeriodDays = 20,
        };
        var mrConfig2 = new MeanReversionConfig(1.0m, 10);
        var parent1 = new MeanReversionChromosome(config1, mrConfig1);
        var parent2 = new MeanReversionChromosome(config2, mrConfig2);

        // Act
        var child = (MeanReversionChromosome)parent1.Crossover(parent2);

        // Assert
        var validMultipliers = new[]
        {
            mrConfig1.DeviationMultiplier,
            mrConfig2.DeviationMultiplier,
        };
        validMultipliers.ShouldContain(child.MeanReversionConfig.DeviationMultiplier);

        var validTimeFrames = new[] { mrConfig1.MeanTimeFrameValue, mrConfig2.MeanTimeFrameValue };
        validTimeFrames.ShouldContain(child.MeanReversionConfig.MeanTimeFrameValue);
    }

    [Fact]
    public void ApplyConfigOverrides_WhenMeanReversion_ShouldSetMeanReversionOverride()
    {
        // Arrange
        var config = new TradingConfiguration();
        var meanReversionConfig = new MeanReversionConfig(
            DeviationMultiplier: 2.0m,
            MeanTimeFrameValue: 20
        );
        var chromosome = new MeanReversionChromosome(config, meanReversionConfig);
        var strategy = Strategy.Create(
            DatabaseExtensions.CreateId<Market>(),
            "Test",
            null,
            StrategyType.MeanReversion,
            new TradingConfiguration()
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
        result.MeanReversionConfigOverride.ShouldNotBeNull();
        result.MeanReversionConfigOverride.ShouldBeSameAs(meanReversionConfig);
        result.SignalWeightedConfigOverride.ShouldBeNull();
        result.ForecastMomentumConfigOverride.ShouldBeNull();
        result.RecipeArbitrageConfigOverride.ShouldBeNull();
    }
}
