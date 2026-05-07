using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest.Schemas;

public sealed record GetBacktestResponse(
    Id<Backtest> Id,
    Id<Strategy> StrategyId,
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    BacktestKind Kind,
    BacktestStatus Status,
    int ProgressPercent,
    string? ProgressMessage,
    BacktestResults? Results,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
