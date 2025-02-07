namespace Ouranos.Pantheon.Core.Common.GeneticAlgorithms;

public interface IGeneticAlgorithmEngineBuilder<T>
{
    IGeneticAlgorithmEngineBuilder<T> SetElitismRate(double elitismRate);

    IGeneticAlgorithmEngineBuilder<T> SetMutationRate(double mutationRate);

    IGeneticAlgorithmEngineBuilder<T> SetPopulationSize(uint populationSize);

    IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(Func<IChromosome<T>, double> fitnessFunction);

    IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(double weight, Func<IChromosome<T>, double> fitnessFunction);

    IGeneticAlgorithmEngine<T> Build();
}