using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class CompositeChromosome(TradingConfiguration configuration)
    : StrategyChromosome(configuration)
{
    public override BacktestParameters ApplyConfigOverrides(BacktestParameters parameters)
    {
        return parameters;
    }

    public override void Mutate(double mutationRate)
    {
        MutateCommonFields(Random.Shared, mutationRate);
    }

    public override IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not CompositeChromosome otherChromosome)
        {
            throw new InvalidOperationException(
                $"Crossover partner must be a {nameof(CompositeChromosome)}."
            );
        }

        var random = Random.Shared;
        var childConfig = CrossoverCommonFields(
            Configuration,
            otherChromosome.Configuration,
            random
        );

        return new CompositeChromosome(childConfig);
    }

    protected override void AddStrategySpecificGenes(List<double> genes) { }
}
