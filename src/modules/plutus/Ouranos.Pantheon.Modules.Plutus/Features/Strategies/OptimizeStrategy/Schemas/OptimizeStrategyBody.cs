using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;

public sealed record OptimizeStrategyBody(
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    int Generations = 30,
    int PopulationSize = 50,
    double DrawdownWeight = 0.5,
    double OutSampleRatio = 0.2,
    decimal? VolumeParticipationRate = null,
    decimal? SlippageMultiplier = null,
    int MinTradesPerMonth = 10
);
