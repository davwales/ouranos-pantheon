namespace Ouranos.Pantheon.Core.Common.GeneticAlgorithms;

public sealed class GeneticAlgorithmEngine<T>(
    int populationSize = 100,
    double mutationRate = 0.01,
    double elitismRate = 0.1
) : IGeneticAlgorithmEngine<T>
{
    private readonly List<FitnessComponent> _fitnessComponents = [];
    private readonly Random _random = new();

    public void AddFitnessComponent(Func<IChromosome<T>, double> fitnessFunction)
    {
        _fitnessComponents.Add(new FitnessComponent(1, fitnessFunction));
    }

    public void AddFitnessComponent(double weight, Func<IChromosome<T>, double> fitnessFunction)
    {
        _fitnessComponents.Add(new FitnessComponent(weight, fitnessFunction));
    }

    public double EvaluateFitness(IChromosome<T> chromosome)
    {
        return _fitnessComponents.Sum(component => component.Weight * component.FitnessFunction(chromosome));
    }

    public IChromosome<T> Evolve(
        ICollection<IChromosome<T>> population,
        int generations,
        double targetFitness = double.MaxValue,
        Action<int, IReadOnlyCollection<IChromosome<T>>>? onGenerationCompleted = null,
        CancellationToken cancellationToken = default
    )
    {
        var currentPopulation = population;

        for (var generation = 0; generation < generations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Evaluate fitness for all chromosomes
            var evaluatedChromosomes = currentPopulation
                .Select(chromosome => new EvaluatedChromosome(EvaluateFitness(chromosome), chromosome))
                .OrderByDescending(evaluatedChromosome => evaluatedChromosome.Fitness)
                .ToList();

            // Check if we've reached target fitness
            if (evaluatedChromosomes[0].Fitness >= targetFitness)
            {
                return evaluatedChromosomes[0].Chromosome;
            }

            var newPopulation = new List<IChromosome<T>>();

            // Elitism - carry over best performing chromosomes
            var eliteCount = (int)(populationSize * elitismRate);
            newPopulation.AddRange(evaluatedChromosomes.Take(eliteCount).Select(x => x.Chromosome));

            // Fill rest of population through selection and crossover
            while (newPopulation.Count < populationSize)
            {
                var parent1 = SelectParent(evaluatedChromosomes);
                var parent2 = SelectParent(evaluatedChromosomes);
                var offspring = parent1.Crossover(parent2);
                offspring.Mutate(mutationRate);
                newPopulation.Add(offspring);
            }

            currentPopulation = newPopulation;
            onGenerationCompleted?.Invoke(generation, currentPopulation.ToList().AsReadOnly());
        }

        return currentPopulation
            .OrderByDescending(EvaluateFitness)
            .First();
    }

    private IChromosome<T> SelectParent(IReadOnlyList<EvaluatedChromosome> evaluatedChromosomes)
    {
        // Tournament selection
        const int tournamentSize = 3;

        var tournament = Enumerable
            .Range(0, tournamentSize)
            .Select(_ => evaluatedChromosomes[_random.Next(evaluatedChromosomes.Count)])
            .OrderByDescending(x => x.Fitness)
            .ToList();

        return tournament[0].Chromosome;
    }

    private record FitnessComponent(double Weight, Func<IChromosome<T>, double> FitnessFunction);

    private record EvaluatedChromosome(double Fitness, IChromosome<T> Chromosome);
}