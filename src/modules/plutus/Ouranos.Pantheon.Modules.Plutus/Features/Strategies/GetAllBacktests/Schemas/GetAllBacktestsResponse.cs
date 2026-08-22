using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests.Schemas;

public sealed record GetAllBacktestsResponse(
    Id<Backtest> Id,
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    BacktestKind Kind,
    BacktestStatus Status,
    decimal? TotalReturnPercent,
    decimal? WinRate,
    decimal? SharpeRatio,
    int? TotalTrades,
    DateTimeOffset CreatedAt
);
