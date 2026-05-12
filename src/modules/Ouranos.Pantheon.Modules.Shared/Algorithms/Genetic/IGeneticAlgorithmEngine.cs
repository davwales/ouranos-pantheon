namespace Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

public interface IGeneticAlgorithmEngine<T>
{
    public double EvaluateFitness(IChromosome<T> chromosome);

    /// <summary>
    /// Evaluates fitness for a chromosome using both sync and async fitness components.
    /// </summary>
    public Task<double> EvaluateFitnessAsync(IChromosome<T> chromosome);

    public IChromosome<T> Evolve(
        IReadOnlyCollection<IChromosome<T>> population,
        uint generations,
        double targetFitness = double.MaxValue,
        Action<int, IReadOnlyCollection<IChromosome<T>>>? onGenerationCompleted = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Evolves the population asynchronously, evaluating fitness using async fitness functions.
    /// </summary>
    /// <param name="population">The initial population of chromosomes.</param>
    /// <param name="generations">The number of generations to evolve.</param>
    /// <param name="targetFitness">Optional target fitness threshold to stop early.</param>
    /// <param name="onGenerationCompletedAsync">Optional async callback invoked after each generation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The best chromosome from the final (or target-achieving) generation.</returns>
    public Task<IChromosome<T>> EvolveAsync(
        IReadOnlyCollection<IChromosome<T>> population,
        uint generations,
        double targetFitness = double.MaxValue,
        Func<int, IReadOnlyCollection<IChromosome<T>>, Task>? onGenerationCompletedAsync = null,
        CancellationToken cancellationToken = default
    );
}
