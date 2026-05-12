using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class MeanReversionChromosome(
    TradingConfiguration configuration,
    MeanReversionConfig meanReversionConfig
) : StrategyChromosome(configuration)
{
    public MeanReversionConfig MeanReversionConfig { get; private set; } = meanReversionConfig;

    public MeanReversionChromosome(TradingConfiguration configuration)
        : this(configuration, new MeanReversionConfig()) { }

    public override BacktestParameters ApplyConfigOverrides(BacktestParameters parameters)
    {
        return parameters with { MeanReversionConfigOverride = MeanReversionConfig };
    }

    public override void Mutate(double mutationRate)
    {
        var random = Random.Shared;
        MutateCommonFields(random, mutationRate);

        if (random.NextDouble() < mutationRate)
        {
            MutateMeanReversionFields(random, mutationRate);
        }
    }

    public override IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not MeanReversionChromosome otherChromosome)
        {
            throw new InvalidOperationException(
                $"Crossover partner must be a {nameof(MeanReversionChromosome)}."
            );
        }

        var random = Random.Shared;
        var childConfig = CrossoverCommonFields(
            Configuration,
            otherChromosome.Configuration,
            random
        );

        var childMeanReversion = new MeanReversionConfig(
            random.NextDouble() < 0.5
                ? MeanReversionConfig.DeviationMultiplier
                : otherChromosome.MeanReversionConfig.DeviationMultiplier,
            random.NextDouble() < 0.5
                ? MeanReversionConfig.MeanTimeFrameValue
                : otherChromosome.MeanReversionConfig.MeanTimeFrameValue
        );

        return new MeanReversionChromosome(childConfig, childMeanReversion);
    }

    protected override void AddStrategySpecificGenes(List<double> genes)
    {
        if (MeanReversionConfig.DeviationMultiplier.HasValue)
        {
            genes.Add((double)MeanReversionConfig.DeviationMultiplier.Value);
        }

        if (MeanReversionConfig.MeanTimeFrameValue.HasValue)
        {
            genes.Add(MeanReversionConfig.MeanTimeFrameValue.Value);
        }
    }

    private void MutateMeanReversionFields(Random random, double mutationRate)
    {
        var current = MeanReversionConfig;

        var deviationMultiplier = current.DeviationMultiplier.HasValue
            ? MutateWeight(current.DeviationMultiplier.Value, random, mutationRate)
            : (decimal?)null;
        var meanTimeFrameValue = current.MeanTimeFrameValue.HasValue
            ? Math.Max(1, current.MeanTimeFrameValue.Value + random.Next(-5, 6))
            : (int?)null;

        MeanReversionConfig = new MeanReversionConfig(deviationMultiplier, meanTimeFrameValue);
    }
}
