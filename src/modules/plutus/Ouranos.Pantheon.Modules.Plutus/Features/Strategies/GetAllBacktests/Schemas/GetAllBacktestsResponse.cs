using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests.Schemas;

public sealed record GetAllBacktestsResponse(
    Id<Backtest> Id,
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    BacktestStatus Status,
    decimal? TotalReturnPercent,
    decimal? WinRate,
    decimal? SharpeRatio,
    int? TotalTrades,
    DateTimeOffset CreatedAt
);
