using Ouranos.Pantheon.Core.Common.GeneticAlgorithms;

namespace Ouranos.Pantheon.Core.Common.Examples.GeneticAlgorithms;

public sealed class GeneticAlgorithmExample : IExample
{
    public void Run()
    {
        var engine = new GeneticAlgorithmEngine<bool>();

        // Truthy Score
        engine.AddFitnessComponent(chromosome => chromosome.Genes.Sum(x => x ? 1 : 0));

        // Repeat Penalty
        engine.AddFitnessComponent(-0.6, chromosome =>
        {
            if (chromosome.Genes.Length is 0 or 1)
            {
                return 0;
            }

            var repeats = 0;
            for (var i = 1; i < chromosome.Genes.Length; i++)
            {
                if (chromosome.Genes[i] == chromosome.Genes[i - 1])
                {
                    repeats++;
                }
            }

            return repeats;
        });

        var population = Enumerable
            .Range(0, 100)
            .Select(IChromosome<bool> (_) => new BinaryChromosome(100))
            .ToList();

        var finalChromosome = engine.Evolve(
            population,
            100000,
            onGenerationCompleted: (generation, generationPopulation) =>
            {
                if (generation % 10 != 0)
                {
                    return;
                }

                var bestFitness = generationPopulation.Select(c => engine.EvaluateFitness(c)).OrderByDescending(x => x)
                    .First();
                Console.WriteLine($"Generation {generation}, Fitness {bestFitness}");
            }
        );

        Console.WriteLine($"Final Fitness {engine.EvaluateFitness(finalChromosome)}");
    }
}