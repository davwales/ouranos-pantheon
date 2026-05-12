using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class SignalWeightedChromosome(
    TradingConfiguration configuration,
    SignalWeightedConfig signalWeightedConfig
) : StrategyChromosome(configuration)
{
    public SignalWeightedConfig SignalWeightedConfig { get; private set; } = signalWeightedConfig;

    public SignalWeightedChromosome(TradingConfiguration configuration)
        : this(configuration, new SignalWeightedConfig()) { }

    public override BacktestParameters ApplyConfigOverrides(BacktestParameters parameters)
    {
        return parameters with { SignalWeightedConfigOverride = SignalWeightedConfig };
    }

    public override void Mutate(double mutationRate)
    {
        var random = Random.Shared;
        MutateCommonFields(random, mutationRate);
        MutateSignalWeightFields(random, mutationRate);
    }

    public override IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not SignalWeightedChromosome otherChromosome)
        {
            throw new InvalidOperationException(
                $"Crossover partner must be a {nameof(SignalWeightedChromosome)}."
            );
        }

        var random = Random.Shared;
        var childConfig = CrossoverCommonFields(
            Configuration,
            otherChromosome.Configuration,
            random
        );
        var childWeights = CrossoverSignalWeightFields(
            SignalWeightedConfig,
            otherChromosome.SignalWeightedConfig,
            random
        );

        return new SignalWeightedChromosome(childConfig, childWeights);
    }

    protected override void AddStrategySpecificGenes(List<double> genes)
    {
        foreach (var weight in SignalWeightedConfig.GetSignalWeights())
        {
            genes.Add((double)weight.Weight);
        }
    }

    private void MutateSignalWeightFields(Random random, double mutationRate)
    {
        if (random.NextDouble() >= mutationRate)
        {
            return;
        }

        var current = SignalWeightedConfig;
        var taxAdjustedRoi = MutateWeight(current.TaxAdjustedRoiWeight ?? 0, random, mutationRate);
        var volumeAnomaly = MutateWeight(current.VolumeAnomalyWeight ?? 0, random, mutationRate);
        var trendMomentum = MutateWeight(current.TrendMomentumWeight ?? 0, random, mutationRate);
        var bollingerBands = MutateWeight(current.BollingerBandsWeight ?? 0, random, mutationRate);
        var rsi = MutateWeight(current.RsiWeight ?? 0, random, mutationRate);
        var movingAverageCrossover = MutateWeight(
            current.MovingAverageCrossoverWeight ?? 0,
            random,
            mutationRate
        );
        var priceVelocity = MutateWeight(current.PriceVelocityWeight ?? 0, random, mutationRate);

        SignalWeightedConfig = new SignalWeightedConfig(
            current.BuyThreshold,
            current.SellThreshold,
            taxAdjustedRoi,
            volumeAnomaly,
            trendMomentum,
            bollingerBands,
            rsi,
            movingAverageCrossover,
            priceVelocity
        );
    }

    private static SignalWeightedConfig CrossoverSignalWeightFields(
        SignalWeightedConfig parent1,
        SignalWeightedConfig parent2,
        Random random
    )
    {
        return new SignalWeightedConfig(
            random.NextDouble() < 0.5 ? parent1.BuyThreshold : parent2.BuyThreshold,
            random.NextDouble() < 0.5 ? parent1.SellThreshold : parent2.SellThreshold,
            random.NextDouble() < 0.5 ? parent1.TaxAdjustedRoiWeight : parent2.TaxAdjustedRoiWeight,
            random.NextDouble() < 0.5 ? parent1.VolumeAnomalyWeight : parent2.VolumeAnomalyWeight,
            random.NextDouble() < 0.5 ? parent1.TrendMomentumWeight : parent2.TrendMomentumWeight,
            random.NextDouble() < 0.5 ? parent1.BollingerBandsWeight : parent2.BollingerBandsWeight,
            random.NextDouble() < 0.5 ? parent1.RsiWeight : parent2.RsiWeight,
            random.NextDouble() < 0.5
                ? parent1.MovingAverageCrossoverWeight
                : parent2.MovingAverageCrossoverWeight,
            random.NextDouble() < 0.5 ? parent1.PriceVelocityWeight : parent2.PriceVelocityWeight
        );
    }
}
