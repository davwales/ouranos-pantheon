using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;

public sealed record OptimizeStrategyBody(
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    int Generations = 100,
    int PopulationSize = 50,
    double SortinoWeight = 0.4,
    double CagrWeight = 0.3,
    double DrawdownWeight = 0.5,
    double TurnoverWeight = 0.1,
    double L1RegularizationWeight = 0.05,
    double OutSampleRatio = 0.2,
    decimal? VolumeParticipationRate = null,
    decimal? SlippageMultiplier = null,
    int MinTrades = 5
);
