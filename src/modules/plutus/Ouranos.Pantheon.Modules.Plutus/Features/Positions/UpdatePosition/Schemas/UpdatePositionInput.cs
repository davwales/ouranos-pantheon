using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition.Schemas;

public sealed record UpdatePositionInput(
    Id<Position> PositionId,
    decimal Cost,
    decimal Quantity,
    string? Notes
);