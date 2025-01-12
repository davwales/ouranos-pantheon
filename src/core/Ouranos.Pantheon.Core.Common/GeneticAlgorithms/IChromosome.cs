namespace Ouranos.Pantheon.Core.Common.GeneticAlgorithms;

public interface IChromosome<T>
{
    T[] Genes { get; }
    
    void Mutate(double mutationRate);
    
    IChromosome<T> Crossover(IChromosome<T> other);
}