using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies.Schemas;

public sealed record GetAllStrategiesResponse(
    Id<Strategy> Id,
    Id<Market> MarketId,
    string Name,
    string? Description,
    StrategyType Type,
    bool IsActive,
    DateTimeOffset CreatedAt,
    int BacktestCount,
    decimal? LastBacktestReturn,
    decimal? LastBacktestWinRate
);
