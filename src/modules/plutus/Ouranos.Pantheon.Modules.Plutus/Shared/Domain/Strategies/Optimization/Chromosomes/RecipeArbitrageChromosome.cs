using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class RecipeArbitrageChromosome(
    TradingConfiguration configuration,
    RecipeArbitrageConfig recipeArbitrageConfig
) : StrategyChromosome(configuration)
{
    public RecipeArbitrageConfig RecipeArbitrageConfig { get; private set; } = recipeArbitrageConfig;

    public RecipeArbitrageChromosome(TradingConfiguration configuration)
        : this(configuration, new RecipeArbitrageConfig())
    {
    }

    public override BacktestParameters ApplyConfigOverrides(BacktestParameters parameters)
    {
        return parameters with { RecipeArbitrageConfigOverride = RecipeArbitrageConfig };
    }

    public override void Mutate(double mutationRate)
    {
        var random = Random.Shared;
        MutateCommonFields(random, mutationRate);

        if (random.NextDouble() < mutationRate)
        {
            MutateRecipeArbitrageFields(random, mutationRate);
        }
    }

    public override IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not RecipeArbitrageChromosome otherChromosome)
        {
            throw new InvalidOperationException($"Crossover partner must be a {nameof(RecipeArbitrageChromosome)}.");
        }

        var random = Random.Shared;
        var childConfig = CrossoverCommonFields(Configuration, otherChromosome.Configuration, random);

        var childRecipe = new RecipeArbitrageConfig(
            random.NextDouble() < 0.5
                ? RecipeArbitrageConfig.MinMarginPercent
                : otherChromosome.RecipeArbitrageConfig.MinMarginPercent
        );

        return new RecipeArbitrageChromosome(childConfig, childRecipe);
    }

    protected override void AddStrategySpecificGenes(List<double> genes)
    {
        if (RecipeArbitrageConfig.MinMarginPercent.HasValue)
        {
            genes.Add((double)RecipeArbitrageConfig.MinMarginPercent.Value);
        }
    }

    private void MutateRecipeArbitrageFields(Random random, double mutationRate)
    {
        var current = RecipeArbitrageConfig;

        var minMarginPercent = current.MinMarginPercent.HasValue
            ? MutateWeight(current.MinMarginPercent.Value, random, mutationRate)
            : (decimal?)null;

        RecipeArbitrageConfig = new RecipeArbitrageConfig(minMarginPercent);
    }
}
