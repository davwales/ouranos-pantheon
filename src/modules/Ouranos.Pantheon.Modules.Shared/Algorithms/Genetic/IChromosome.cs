namespace Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

public interface IChromosome<T>
{
    T[] Genes { get; }

    void Mutate(double mutationRate);

    IChromosome<T> Crossover(IChromosome<T> other);
}