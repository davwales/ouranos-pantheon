using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests.Schemas;

public sealed record GetActiveBacktestsResponse(
    Id<Backtest> Id,
    Id<Strategy> StrategyId,
    string StrategyName,
    Id<Market> MarketId,
    BacktestKind Kind,
    BacktestStatus Status,
    int ProgressPercent,
    string? ProgressMessage,
    DateTimeOffset CreatedAt
);
