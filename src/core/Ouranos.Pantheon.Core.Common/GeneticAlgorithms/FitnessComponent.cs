namespace Ouranos.Pantheon.Core.Common.GeneticAlgorithms;

public sealed record FitnessComponent<T>(
    double Weight,
    Func<IChromosome<T>, double> FitnessFunction
);