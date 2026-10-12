using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

/// <summary>One candidate configuration: a value for every parameter in a <see cref="ParameterSpace" />.</summary>
public sealed class ParameterChromosome : IChromosome<double>
{
    /// <summary>Largest mutation step, as a share of the parameter's range.</summary>
    private const double MaxMutationStep = 0.15;

    private readonly double[] _genes;

    public ParameterChromosome(ParameterSpace space)
        : this(space, space.Seed) { }

    private ParameterChromosome(ParameterSpace space, IEnumerable<double> genes)
    {
        Guard.Against.Null(space);

        Space = space;
        _genes = [.. genes.Select((gene, i) => space.Parameters[i].Clamp(gene))];
    }

    public ParameterSpace Space { get; }

    public double[] Genes => [.. _genes];

    public StrategyConfiguration Configuration => Space.Build(_genes);

    public static ParameterChromosome CreateRandom(ParameterSpace space)
    {
        Guard.Against.Null(space);

        var random = Random.Shared;
        return new ParameterChromosome(
            space,
            space.Parameters.Select(p => p.Min + (random.NextDouble() * (p.Max - p.Min)))
        );
    }

    /// <summary>
    ///     Each gene mutates independently with probability <paramref name="mutationRate" />,
    ///     but at least one gene per call on average.
    /// </summary>
    public void Mutate(double mutationRate)
    {
        if (_genes.Length == 0)
        {
            return;
        }

        var random = Random.Shared;
        var probability = Math.Max(mutationRate, 1.0 / _genes.Length);
        for (var i = 0; i < _genes.Length; i++)
        {
            if (random.NextDouble() >= probability)
            {
                continue;
            }

            var parameter = Space.Parameters[i];

            var step =
                ((random.NextDouble() * 2) - 1) * MaxMutationStep * (parameter.Max - parameter.Min);

            _genes[i] = parameter.Clamp(_genes[i] + step);
        }
    }

    public IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not ParameterChromosome partner || partner.Space != Space)
        {
            throw new InvalidOperationException(
                "Crossover partner must come from the same parameter space."
            );
        }

        var random = Random.Shared;
        return new ParameterChromosome(
            Space,
            _genes.Select((gene, i) => random.NextDouble() < 0.5 ? gene : partner._genes[i])
        );
    }
}
