using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;

public sealed record OptimizeStrategyInput(
    Id<Strategy> StrategyId,
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    int Generations = 20,
    int PopulationSize = 20,
    double SharpeRatioWeight = 0.5,
    double TotalReturnWeight = 0.3,
    double MaxDrawdownWeight = -0.2,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m
);
