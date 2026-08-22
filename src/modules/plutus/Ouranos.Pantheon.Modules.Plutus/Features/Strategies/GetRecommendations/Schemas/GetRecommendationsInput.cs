using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

public sealed record GetRecommendationsInput(
    Id<Strategy> StrategyId,
    Id<Market> MarketId,
    decimal Budget
);
