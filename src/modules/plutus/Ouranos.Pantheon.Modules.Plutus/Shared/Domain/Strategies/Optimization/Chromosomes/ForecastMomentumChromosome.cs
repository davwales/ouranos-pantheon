using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class ForecastMomentumChromosome(
    TradingConfiguration configuration,
    ForecastMomentumConfig forecastMomentumConfig
) : StrategyChromosome(configuration)
{
    public ForecastMomentumConfig ForecastMomentumConfig { get; private set; } = forecastMomentumConfig;

    public ForecastMomentumChromosome(TradingConfiguration configuration)
        : this(configuration, new ForecastMomentumConfig())
    {
    }

    public override BacktestParameters ApplyConfigOverrides(BacktestParameters parameters)
    {
        return parameters with { ForecastMomentumConfigOverride = ForecastMomentumConfig };
    }

    public override void Mutate(double mutationRate)
    {
        var random = Random.Shared;
        MutateCommonFields(random, mutationRate);

        if (random.NextDouble() < mutationRate)
        {
            MutateForecastMomentumFields(random, mutationRate);
        }
    }

    public override IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not ForecastMomentumChromosome otherChromosome)
        {
            throw new InvalidOperationException($"Crossover partner must be a {nameof(ForecastMomentumChromosome)}.");
        }

        var random = Random.Shared;
        var childConfig = CrossoverCommonFields(Configuration, otherChromosome.Configuration, random);

        var childForecast = new ForecastMomentumConfig(
            random.NextDouble() < 0.5
                ? ForecastMomentumConfig.ForecastMovementThreshold
                : otherChromosome.ForecastMomentumConfig.ForecastMovementThreshold,
            random.NextDouble() < 0.5
                ? ForecastMomentumConfig.ForecastHorizonDays
                : otherChromosome.ForecastMomentumConfig.ForecastHorizonDays
        );

        return new ForecastMomentumChromosome(childConfig, childForecast);
    }

    protected override void AddStrategySpecificGenes(List<double> genes)
    {
        if (ForecastMomentumConfig.ForecastMovementThreshold.HasValue)
        {
            genes.Add((double)ForecastMomentumConfig.ForecastMovementThreshold.Value);
        }

        if (ForecastMomentumConfig.ForecastHorizonDays.HasValue)
        {
            genes.Add(ForecastMomentumConfig.ForecastHorizonDays.Value);
        }
    }

    private void MutateForecastMomentumFields(Random random, double mutationRate)
    {
        var current = ForecastMomentumConfig;

        var forecastMovementThreshold = current.ForecastMovementThreshold.HasValue
            ? MutateWeight(current.ForecastMovementThreshold.Value, random, mutationRate)
            : (decimal?)null;
        var forecastHorizonDays = current.ForecastHorizonDays.HasValue
            ? Math.Max(1, current.ForecastHorizonDays.Value + random.Next(-3, 4))
            : (int?)null;

        ForecastMomentumConfig = new ForecastMomentumConfig(forecastMovementThreshold, forecastHorizonDays);
    }
}
