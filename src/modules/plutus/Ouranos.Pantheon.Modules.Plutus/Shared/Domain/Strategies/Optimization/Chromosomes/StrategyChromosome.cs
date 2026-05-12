using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

public abstract class StrategyChromosome(TradingConfiguration configuration) : IChromosome<double>
{
    public TradingConfiguration Configuration { get; private set; } = configuration;

    public double[] Genes => ToGenes();

    public abstract void Mutate(double mutationRate);
    public abstract IChromosome<double> Crossover(IChromosome<double> other);
    public abstract BacktestParameters ApplyConfigOverrides(BacktestParameters parameters);
    protected abstract void AddStrategySpecificGenes(List<double> genes);

    public static StrategyChromosome Create(StrategyType strategyType, TradingConfiguration configuration)
    {
        return strategyType switch
        {
            StrategyType.SignalWeighted => new SignalWeightedChromosome(configuration),
            StrategyType.ForecastMomentum => new ForecastMomentumChromosome(configuration),
            StrategyType.MeanReversion => new MeanReversionChromosome(configuration),
            StrategyType.RecipeArbitrage => new RecipeArbitrageChromosome(configuration),
            _ => new CompositeChromosome(configuration)
        };
    }

    public static StrategyChromosome CreateRandom(StrategyType strategyType)
    {
        var random = Random.Shared;
        var configuration = new TradingConfiguration
        {
            MaxPositions = random.Next(1, 20),
            MaxPositionPercent = 0.05m + (decimal)random.NextDouble() * 0.45m,
            HoldPeriodDays = random.Next(1, 30)
        };

        return strategyType switch
        {
            StrategyType.SignalWeighted => new SignalWeightedChromosome(
                configuration,
                CreateRandomSignalWeighted(random)
            ),
            StrategyType.ForecastMomentum => new ForecastMomentumChromosome(
                configuration,
                CreateRandomForecastMomentum(random)
            ),
            StrategyType.MeanReversion => new MeanReversionChromosome(configuration, CreateRandomMeanReversion(random)),
            StrategyType.RecipeArbitrage => new RecipeArbitrageChromosome(
                configuration,
                CreateRandomRecipeArbitrage(random)
            ),
            _ => new CompositeChromosome(configuration)
        };
    }

    protected void MutateCommonFields(Random random, double mutationRate)
    {
        if (random.NextDouble() >= mutationRate)
        {
            return;
        }

        var field = random.Next(3);

        if (field == 0 && Configuration.MaxPositions.HasValue)
        {
            Configuration = Configuration with
            {
                MaxPositions = Math.Max(1, Configuration.MaxPositions.Value + random.Next(-3, 4))
            };
        }
        else if (field == 1 && Configuration.MaxPositionPercent.HasValue)
        {
            Configuration = Configuration with
            {
                MaxPositionPercent = Math.Clamp(
                    Configuration.MaxPositionPercent.Value + (decimal)(random.NextDouble() - 0.5) * 0.1m,
                    0.05m,
                    0.5m
                )
            };
        }
        else if (field == 2 && Configuration.HoldPeriodDays.HasValue)
        {
            Configuration = Configuration with
            {
                HoldPeriodDays = Math.Max(1, Configuration.HoldPeriodDays.Value + random.Next(-5, 6))
            };
        }
    }

    protected TradingConfiguration CrossoverCommonFields(
        TradingConfiguration parent1,
        TradingConfiguration parent2,
        Random random
    )
    {
        return new TradingConfiguration
        {
            MaxPositions = random.NextDouble() < 0.5 ? parent1.MaxPositions : parent2.MaxPositions,
            MaxPositionPercent =
                random.NextDouble() < 0.5 ? parent1.MaxPositionPercent : parent2.MaxPositionPercent,
            HoldPeriodDays = random.NextDouble() < 0.5 ? parent1.HoldPeriodDays : parent2.HoldPeriodDays,
        };
    }

    protected static decimal MutateWeight(decimal current, Random random, double mutationRate)
    {
        return Math.Clamp(
            current + (random.NextDouble() < mutationRate ? (decimal)(random.NextDouble() - 0.5) * 0.4m : 0m),
            0m,
            3m
        );
    }

    private double[] ToGenes()
    {
        var genes = new List<double>();

        AddGeneIfHasValue(genes, Configuration.MaxPositions);
        AddGeneIfHasValue(genes, Configuration.MaxPositionPercent, v => (double)v);
        AddGeneIfHasValue(genes, Configuration.HoldPeriodDays, v => v);
        AddStrategySpecificGenes(genes);

        return [.. genes];
    }

    protected static void AddGeneIfHasValue<T>(List<double> genes, T? value, Func<T, double> convert) where T : struct
    {
        if (value.HasValue)
        {
            genes.Add(convert(value.Value));
        }
    }

    protected static void AddGeneIfHasValue(List<double> genes, int? value)
    {
        if (value.HasValue)
        {
            genes.Add(value.Value);
        }
    }

    private static SignalWeightedConfig CreateRandomSignalWeighted(Random random)
    {
        return new SignalWeightedConfig(
            (decimal)random.NextDouble() * 100,
            (decimal)random.NextDouble() * 100,
            (decimal)random.NextDouble() * 2,
            (decimal)random.NextDouble() * 2,
            (decimal)random.NextDouble() * 2,
            (decimal)random.NextDouble() * 2,
            (decimal)random.NextDouble() * 2,
            (decimal)random.NextDouble() * 2,
            (decimal)random.NextDouble() * 2
        );
    }

    private static ForecastMomentumConfig CreateRandomForecastMomentum(Random random)
    {
        return new ForecastMomentumConfig(
            (decimal)random.NextDouble() * 2,
            random.Next(1, 30)
        );
    }

    private static MeanReversionConfig CreateRandomMeanReversion(Random random)
    {
        return new MeanReversionConfig(
            (decimal)(random.NextDouble() * 2 + 0.5),
            random.Next(1, 30)
        );
    }

    private static RecipeArbitrageConfig CreateRandomRecipeArbitrage(Random random)
    {
        return new RecipeArbitrageConfig((decimal)(random.NextDouble() * 0.5 + 0.01));
    }
}
