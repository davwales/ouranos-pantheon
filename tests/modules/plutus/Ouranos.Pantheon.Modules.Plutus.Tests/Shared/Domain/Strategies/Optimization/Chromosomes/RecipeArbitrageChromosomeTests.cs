using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class RecipeArbitrageChromosomeTests
{
    [Fact]
    public void CreateRandom_WhenRecipeArbitrage_ShouldSetCommonFieldsOnly()
    {
        // Arrange & Act
        var chromosome = (RecipeArbitrageChromosome)StrategyChromosome.CreateRandom(StrategyType.RecipeArbitrage);

        // Assert
        chromosome.Configuration.MaxPositions.ShouldNotBeNull();
        chromosome.Configuration.MaxPositions.Value.ShouldBeInRange(1, 19);
        chromosome.Configuration.MaxPositionPercent.ShouldNotBeNull();
        chromosome.Configuration.MaxPositionPercent.Value.ShouldBeInRange(0.05m, 0.50m);
        chromosome.Configuration.HoldPeriodDays.ShouldNotBeNull();
        chromosome.Configuration.HoldPeriodDays.Value.ShouldBeInRange(1, 29);
    }

    [Fact]
    public void Genes_WhenRecipeArbitrage_ShouldReturnCommonPlusOneSpecific()
    {
        // Arrange
        var chromosome = new RecipeArbitrageChromosome(
            new TradingConfiguration { MaxPositions = 5, MaxPositionPercent = 0.1m, HoldPeriodDays = 10 },
            new RecipeArbitrageConfig(0.05m)
        );

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.Length.ShouldBe(4);
    }

    [Fact]
    public void Mutate_WhenRecipeArbitrage_ShouldNotThrow()
    {
        // Arrange
        var chromosome = new RecipeArbitrageChromosome(
            new TradingConfiguration { MaxPositions = 10, MaxPositionPercent = 0.2m, HoldPeriodDays = 15 }
        );

        // Act & Assert
        Should.NotThrow(() => chromosome.Mutate(0.5));
    }

    [Fact]
    public void Crossover_WhenRecipeArbitrage_ChildShouldInheritRecipeArbitrageFields()
    {
        // Arrange
        var config1 = new TradingConfiguration { MaxPositions = 5, MaxPositionPercent = 0.2m, HoldPeriodDays = 10 };
        var recipe1 = new RecipeArbitrageConfig(0.05m);
        var config2 = new TradingConfiguration { MaxPositions = 15, MaxPositionPercent = 0.4m, HoldPeriodDays = 20 };
        var recipe2 = new RecipeArbitrageConfig(0.15m);
        var parent1 = new RecipeArbitrageChromosome(config1, recipe1);
        var parent2 = new RecipeArbitrageChromosome(config2, recipe2);

        // Act
        var child = (RecipeArbitrageChromosome)parent1.Crossover(parent2);

        // Assert
        var validMargins = new[] { recipe1.MinMarginPercent, recipe2.MinMarginPercent };
        validMargins.ShouldContain(child.RecipeArbitrageConfig.MinMarginPercent);
    }

    [Fact]
    public void ApplyConfigOverrides_WhenRecipeArbitrage_ShouldSetRecipeArbitrageOverride()
    {
        // Arrange
        var config = new TradingConfiguration();
        var recipeConfig = new RecipeArbitrageConfig(MinMarginPercent: 0.05m);
        var chromosome = new RecipeArbitrageChromosome(config, recipeConfig);
        var strategy = Strategy.Create(
            DatabaseExtensions.CreateId<Market>(),
            "Test",
            null,
            StrategyType.RecipeArbitrage,
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
        result.RecipeArbitrageConfigOverride.ShouldNotBeNull();
        result.RecipeArbitrageConfigOverride.ShouldBeSameAs(recipeConfig);
        result.SignalWeightedConfigOverride.ShouldBeNull();
        result.ForecastMomentumConfigOverride.ShouldBeNull();
        result.MeanReversionConfigOverride.ShouldBeNull();
    }
}
