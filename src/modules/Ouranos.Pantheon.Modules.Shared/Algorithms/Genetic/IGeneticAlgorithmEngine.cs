namespace Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

public interface IGeneticAlgorithmEngine<T>
{
    public double EvaluateFitness(
        IChromosome<T> chromosome
    );

    public IChromosome<T> Evolve(
        IReadOnlyCollection<IChromosome<T>> population,
        uint generations,
        double targetFitness = double.MaxValue,
        Action<int, IReadOnlyCollection<IChromosome<T>>>? onGenerationCompleted = null,
        CancellationToken cancellationToken = default
    );
}