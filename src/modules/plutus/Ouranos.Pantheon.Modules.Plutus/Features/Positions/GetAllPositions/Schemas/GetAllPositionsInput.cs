using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;

public sealed record GetAllPositionsInput(
    Id<Market> MarketId,
    PositionSide? Side = null,
    PositionStatus? Status = null,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
);
