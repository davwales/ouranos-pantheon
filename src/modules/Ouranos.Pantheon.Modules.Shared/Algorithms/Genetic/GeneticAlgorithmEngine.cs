namespace Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;

public sealed class GeneticAlgorithmEngine<T>(
    uint populationSize,
    double mutationRate,
    double elitismRate,
    IReadOnlyCollection<FitnessComponent<T>> fitnessComponents
) : IGeneticAlgorithmEngine<T>
{
    private readonly Random _random = new();

    public double EvaluateFitness(IChromosome<T> chromosome)
    {
        return fitnessComponents.Sum(component => component.Weight * component.FitnessFunction(chromosome));
    }

    public async Task<double> EvaluateFitnessAsync(IChromosome<T> chromosome)
    {
        double total = 0;
        foreach (var component in fitnessComponents)
        {
            if (component.AsyncFitnessFunction is not null)
            {
                total += component.Weight * await component.AsyncFitnessFunction(chromosome);
            }
            else
            {
                total += component.Weight * component.FitnessFunction(chromosome);
            }
        }

        return total;
    }

    public IChromosome<T> Evolve(
        IReadOnlyCollection<IChromosome<T>> population,
        uint generations,
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

    public async Task<IChromosome<T>> EvolveAsync(
        IReadOnlyCollection<IChromosome<T>> population,
        uint generations,
        double targetFitness = double.MaxValue,
        Func<int, IReadOnlyCollection<IChromosome<T>>, Task>? onGenerationCompletedAsync = null,
        CancellationToken cancellationToken = default
    )
    {
        var currentPopulation = population;

        for (var generation = 0; generation < generations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Evaluate fitness for all chromosomes asynchronously
            var fitnessTasks = currentPopulation
                .Select(async chromosome => new EvaluatedChromosome(
                    await EvaluateFitnessAsync(chromosome),
                    chromosome))
                .ToList();

            var evaluatedChromosomes = (await Task.WhenAll(fitnessTasks))
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

            if (onGenerationCompletedAsync is not null)
            {
                await onGenerationCompletedAsync(generation, currentPopulation.ToList().AsReadOnly());
            }
        }

        // Evaluate final population to find best
        var finalFitnessTasks = currentPopulation
            .Select(async chromosome => new EvaluatedChromosome(
                await EvaluateFitnessAsync(chromosome),
                chromosome))
            .ToList();

        var finalEvaluated = await Task.WhenAll(finalFitnessTasks);

        return finalEvaluated
            .OrderByDescending(e => e.Fitness)
            .First()
            .Chromosome;
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

    private record EvaluatedChromosome(double Fitness, IChromosome<T> Chromosome);
}
