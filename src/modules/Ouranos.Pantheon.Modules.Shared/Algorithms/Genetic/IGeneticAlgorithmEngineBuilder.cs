namespace Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

public interface IGeneticAlgorithmEngineBuilder<T>
{
    IGeneticAlgorithmEngineBuilder<T> SetElitismRate(double elitismRate);

    IGeneticAlgorithmEngineBuilder<T> SetMutationRate(double mutationRate);

    IGeneticAlgorithmEngineBuilder<T> SetPopulationSize(uint populationSize);

    IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(Func<IChromosome<T>, double> fitnessFunction);

    IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(double weight, Func<IChromosome<T>, double> fitnessFunction);

    /// <summary>
    /// Adds an async fitness component with default weight of 1.
    /// </summary>
    IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(Func<IChromosome<T>, Task<double>> asyncFitnessFunction);

    /// <summary>
    /// Adds an async fitness component with the specified weight.
    /// </summary>
    IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(double weight, Func<IChromosome<T>, Task<double>> asyncFitnessFunction);

    IGeneticAlgorithmEngine<T> Build();
}