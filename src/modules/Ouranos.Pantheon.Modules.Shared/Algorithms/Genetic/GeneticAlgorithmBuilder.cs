using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

public sealed class GeneticAlgorithmBuilder<T> : IGeneticAlgorithmEngineBuilder<T>
{
    private readonly List<FitnessComponent<T>> _fitnessComponents = [];
    private readonly List<FitnessComponent<T>> _asyncFitnessComponents = [];
    public double ElitismRate { get; private set; } = 0.1;

    public double MutationRate { get; private set; } = 0.01;

    public uint PopulationSize { get; private set; } = 100;

    public IReadOnlyList<FitnessComponent<T>> FitnessComponents => _fitnessComponents;

    public IGeneticAlgorithmEngineBuilder<T> SetElitismRate(double elitismRate)
    {
        Guard.Against.OutOfRange(elitismRate, nameof(elitismRate), 0, 1);
        ElitismRate = elitismRate;
        return this;
    }

    public IGeneticAlgorithmEngineBuilder<T> SetMutationRate(double mutationRate)
    {
        Guard.Against.OutOfRange(mutationRate, nameof(mutationRate), 0, 1);
        MutationRate = mutationRate;
        return this;
    }

    public IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(
        Func<IChromosome<T>, double> fitnessFunction
    )
    {
        _fitnessComponents.Add(new FitnessComponent<T>(1, fitnessFunction));
        return this;
    }

    public IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(
        double weight,
        Func<IChromosome<T>, double> fitnessFunction
    )
    {
        _fitnessComponents.Add(new FitnessComponent<T>(weight, fitnessFunction));
        return this;
    }

    public IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(
        Func<IChromosome<T>, Task<double>> asyncFitnessFunction
    )
    {
        _asyncFitnessComponents.Add(new FitnessComponent<T>(1, _ => 0, asyncFitnessFunction));
        return this;
    }

    public IGeneticAlgorithmEngineBuilder<T> AddFitnessComponent(
        double weight,
        Func<IChromosome<T>, Task<double>> asyncFitnessFunction
    )
    {
        _asyncFitnessComponents.Add(new FitnessComponent<T>(weight, _ => 0, asyncFitnessFunction));
        return this;
    }

    public IGeneticAlgorithmEngine<T> Build()
    {
        if (_fitnessComponents.Count == 0 && _asyncFitnessComponents.Count == 0)
        {
            throw new InvalidOperationException("No Fitness Components were found.");
        }

        var allComponents = _fitnessComponents.Concat(_asyncFitnessComponents).ToList();

        return new GeneticAlgorithmEngine<T>(
            PopulationSize,
            MutationRate,
            ElitismRate,
            allComponents
        );
    }

    public IGeneticAlgorithmEngineBuilder<T> SetPopulationSize(uint populationSize)
    {
        PopulationSize = populationSize;
        return this;
    }
}
