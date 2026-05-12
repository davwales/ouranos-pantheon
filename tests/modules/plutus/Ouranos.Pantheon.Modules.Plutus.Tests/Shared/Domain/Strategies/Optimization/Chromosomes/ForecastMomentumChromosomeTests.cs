using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class ForecastMomentumChromosomeTests
{
    [Fact]
    public void CreateRandom_WhenForecastMomentum_ShouldSetCommonFieldsOnly()
    {
        // Arrange & Act
        var chromosome = (ForecastMomentumChromosome)
            StrategyChromosome.CreateRandom(StrategyType.ForecastMomentum);

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeInRange(1, 19);
        chromosome.Configuration.MaxPositionPercent.ShouldNotBeNull();
        chromosome.Configuration.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);
        chromosome.Configuration.HoldPeriodDays.ShouldNotBeNull();
        chromosome.Configuration.HoldPeriodDays.Value.ShouldBeInRange(1, 29);
    }

    [Fact]
    public void Genes_WhenForecastMomentum_ShouldReturnCommonPlusTwoSpecific()
    {
        // Arrange
        var chromosome = new ForecastMomentumChromosome(
            new TradingConfiguration
            {
                MaxPositions = 5,
                MaxPositionPercent = 0.1m,
                HoldPeriodDays = 10,
            },
            new ForecastMomentumConfig(1.5m, 14)
        );

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(5);
    }

    [Fact]
    public void Mutate_WhenForecastMomentum_ShouldNotThrow()
    {
        // Arrange
        var chromosome = new ForecastMomentumChromosome(
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
    public void Crossover_WhenForecastMomentum_ChildShouldInheritForecastMomentumFields()
    {
        // Arrange
        var config1 = new TradingConfiguration
        {
            MaxPositions = 5,
            MaxPositionPercent = 0.2m,
            HoldPeriodDays = 10,
        };
        var forecast1 = new ForecastMomentumConfig(1.5m, 14);
        var config2 = new TradingConfiguration
        {
            MaxPositions = 15,
            MaxPositionPercent = 0.4m,
            HoldPeriodDays = 20,
        };
        var forecast2 = new ForecastMomentumConfig(0.5m, 7);
        var parent1 = new ForecastMomentumChromosome(config1, forecast1);
        var parent2 = new ForecastMomentumChromosome(config2, forecast2);

        // Act
        var child = (ForecastMomentumChromosome)parent1.Crossover(parent2);

        // Assert
        var validThresholds = new[]
        {
            forecast1.ForecastMovementThreshold,
            forecast2.ForecastMovementThreshold,
        };
        validThresholds.ShouldContain(child.ForecastMomentumConfig.ForecastMovementThreshold);

        var validHorizons = new[] { forecast1.ForecastHorizonDays, forecast2.ForecastHorizonDays };
        validHorizons.ShouldContain(child.ForecastMomentumConfig.ForecastHorizonDays);
    }

    [Fact]
    public void ApplyConfigOverrides_WhenForecastMomentum_ShouldSetForecastMomentumOverride()
    {
        // Arrange
        var config = new TradingConfiguration();
        var forecastConfig = new ForecastMomentumConfig(
            ForecastMovementThreshold: 1.5m,
            ForecastHorizonDays: 14
        );
        var chromosome = new ForecastMomentumChromosome(config, forecastConfig);
        var strategy = Strategy.Create(
            DatabaseExtensions.CreateId<Market>(),
            "Test",
            null,
            StrategyType.ForecastMomentum,
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
        result.ForecastMomentumConfigOverride.ShouldNotBeNull();
        result.ForecastMomentumConfigOverride.ShouldBeSameAs(forecastConfig);
        result.SignalWeightedConfigOverride.ShouldBeNull();
        result.MeanReversionConfigOverride.ShouldBeNull();
        result.RecipeArbitrageConfigOverride.ShouldBeNull();
    }
}
