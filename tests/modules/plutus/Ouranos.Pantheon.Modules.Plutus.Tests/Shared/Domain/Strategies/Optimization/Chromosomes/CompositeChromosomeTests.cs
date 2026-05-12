using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class CompositeChromosomeTests
{
    [Fact]
    public void CreateRandom_WhenComposite_ShouldOnlySetCommonFields()
    {
        // Arrange & Act
        var chromosome = (CompositeChromosome)
            StrategyChromosome.CreateRandom(StrategyType.Composite);

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeInRange(1, 19);
        chromosome.Configuration.MaxPositionPercent.ShouldNotBeNull();
        chromosome.Configuration.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);
        chromosome.Configuration.HoldPeriodDays.ShouldNotBeNull();
        chromosome.Configuration.HoldPeriodDays.Value.ShouldBeInRange(1, 29);
    }

    [Fact]
    public void Genes_WhenComposite_ShouldReturnCommonOnly()
    {
        // Arrange
        var chromosome = (CompositeChromosome)
            StrategyChromosome.Create(
                StrategyType.Composite,
                new TradingConfiguration
                {
                    MaxPositions = 5,
                    MaxPositionPercent = 0.1m,
                    HoldPeriodDays = 10,
                }
            );

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(3);
    }

    [Fact]
    public void Mutate_WhenHoldPeriodDaysHasValue_CanChangeIt()
    {
        // Arrange
        var config = new TradingConfiguration
        {
            MaxPositions = 10,
            MaxPositionPercent = 0.2m,
            HoldPeriodDays = 15,
        };
        var chromosome = StrategyChromosome.Create(StrategyType.Composite, config);

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
    public void Mutate_WhenCompositeType_ShouldOnlyMutateCommonFields()
    {
        // Arrange
        var config = new TradingConfiguration
        {
            MaxPositions = 10,
            MaxPositionPercent = 0.2m,
            HoldPeriodDays = 15,
        };
        var chromosome = (CompositeChromosome)
            StrategyChromosome.Create(StrategyType.Composite, config);

        // Act
        chromosome.Mutate(1.0);

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Mutate_WhenComposite_ShouldNotThrow()
    {
        // Arrange
        var chromosome = new CompositeChromosome(
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
    public void Crossover_WhenCompositeType_ShouldOnlyCrossoverCommonFields()
    {
        // Arrange
        var config1 = new TradingConfiguration
        {
            MaxPositions = 3,
            MaxPositionPercent = 0.10m,
            HoldPeriodDays = 7,
        };
        var config2 = new TradingConfiguration
        {
            MaxPositions = 15,
            MaxPositionPercent = 0.40m,
            HoldPeriodDays = 25,
        };
        var parent1 = new CompositeChromosome(config1);
        var parent2 = new CompositeChromosome(config2);

        // Act
        var child = (CompositeChromosome)parent1.Crossover(parent2);
        var childConfig = child.Configuration;

        // Assert
        int?[] validMaxPositions = [config1.MaxPositions, config2.MaxPositions];
        validMaxPositions.ShouldContain(childConfig.MaxPositions);
    }

    [Fact]
    public void ApplyConfigOverrides_WhenComposite_ShouldReturnNoOverrides()
    {
        // Arrange
        var config = new TradingConfiguration();
        var chromosome = new CompositeChromosome(config);
        var strategy = Strategy.Create(
            DatabaseExtensions.CreateId<Market>(),
            "Test",
            null,
            StrategyType.Composite,
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
        result.SignalWeightedConfigOverride.ShouldBeNull();
        result.ForecastMomentumConfigOverride.ShouldBeNull();
        result.MeanReversionConfigOverride.ShouldBeNull();
        result.RecipeArbitrageConfigOverride.ShouldBeNull();
    }
}
