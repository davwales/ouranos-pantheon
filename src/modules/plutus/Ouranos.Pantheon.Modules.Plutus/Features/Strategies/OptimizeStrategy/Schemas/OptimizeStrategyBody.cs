using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;

public sealed record OptimizeStrategyBody(
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    int Generations = 100,
    int PopulationSize = 50,
    double SharpeRatioWeight = 0.5,
    double TotalReturnWeight = 0.3,
    double MaxDrawdownWeight = -0.2
);
