using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Algorithms.Genetic;

internal sealed class TestChromosome : IChromosome<bool>
{
    public bool[] Genes { get; } = [true, false, true];

    public void Mutate(double mutationRate)
    {
    }

    public IChromosome<bool> Crossover(IChromosome<bool> other)
    {
        return new TestChromosome();
    }
}