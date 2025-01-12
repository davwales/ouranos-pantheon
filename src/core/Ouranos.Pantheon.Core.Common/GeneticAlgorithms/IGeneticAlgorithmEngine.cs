namespace Ouranos.Pantheon.Core.Common.GeneticAlgorithms;

public interface IGeneticAlgorithmEngine<T>
{
    public void AddFitnessComponent(Func<IChromosome<T>, double> fitnessFunction);
    
    public void AddFitnessComponent(double weight, Func<IChromosome<T>, double> fitnessFunction);
    
    public double EvaluateFitness(IChromosome<T> chromosome);

    public IChromosome<T> Evolve(
        ICollection<IChromosome<T>> population,
        int generations,
        double targetFitness = double.MaxValue,
        Action<int, IReadOnlyCollection<IChromosome<T>>>? onGenerationCompleted = null,
        CancellationToken cancellationToken = default
    );
}