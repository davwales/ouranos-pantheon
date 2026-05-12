namespace Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

public sealed record FitnessComponent<T>(
    double Weight,
    Func<IChromosome<T>, double> FitnessFunction,
    Func<IChromosome<T>, Task<double>>? AsyncFitnessFunction = null
);
