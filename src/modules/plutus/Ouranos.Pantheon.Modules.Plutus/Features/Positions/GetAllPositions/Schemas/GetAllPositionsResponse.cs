using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;

public sealed record GetAllPositionsResponse(
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