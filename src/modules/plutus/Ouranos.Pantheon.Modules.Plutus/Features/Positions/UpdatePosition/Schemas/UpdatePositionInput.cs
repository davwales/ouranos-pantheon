using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition.Schemas;

public sealed record UpdatePositionInput(
    Id<Position> PositionId,
    decimal Cost,
    decimal Quantity,
    string? Notes
);
