using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition.Schemas;

public sealed record GetPositionResponse(
    Id<Position> Id,
    PositionSide Side,
    PositionStatus Status,
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string SymbolName,
    decimal Cost,
    decimal Quantity,
    Id<Position>? LinkedBuyPositionId,
    Id<Strategy>? StrategyId,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
