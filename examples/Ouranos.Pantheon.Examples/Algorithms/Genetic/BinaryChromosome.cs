using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

namespace Ouranos.Pantheon.Examples.Algorithms.Genetic;

public sealed class BinaryChromosome : IChromosome<bool>
{
    private readonly Random _random = new();

    public bool[] Genes { get; }

    public BinaryChromosome(int length)
    {
        Genes = Enumerable.Range(0, length).Select(_ => _random.Next(2) == 1).ToArray();
    }

    public void Mutate(double mutationRate)
    {
        for (var i = 0; i < Genes.Length; i++)
        {
            if (_random.NextDouble() < mutationRate)
            {
                Genes[i] = !Genes[i];
            }
        }
    }

    public IChromosome<bool> Crossover(IChromosome<bool> other)
    {
        var child = new BinaryChromosome(Genes.Length);
        var crossoverPoint = _random.Next(Genes.Length);

        for (var i = 0; i < Genes.Length; i++)
        {
            child.Genes[i] = i < crossoverPoint ? Genes[i] : other.Genes[i];
        }

        return child;
    }
}
