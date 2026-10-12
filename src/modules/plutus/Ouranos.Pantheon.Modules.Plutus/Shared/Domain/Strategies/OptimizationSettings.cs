namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

/// <param name="Generations">Genetic algorithm generations.</param>
/// <param name="PopulationSize">Candidate configurations per generation.</param>
/// <param name="DrawdownWeight">Fitness penalty per unit of max drawdown.</param>
/// <param name="OutSampleRatio">Share of the window held back to validate the winner.</param>
/// <param name="MinTradesPerMonth">Round trips per 30 days below which fitness is penalized.</param>
public sealed record OptimizationSettings(
    int Generations,
    int PopulationSize,
    double DrawdownWeight = 0.5,
    double OutSampleRatio = 0.2,
    int MinTradesPerMonth = 10
);
