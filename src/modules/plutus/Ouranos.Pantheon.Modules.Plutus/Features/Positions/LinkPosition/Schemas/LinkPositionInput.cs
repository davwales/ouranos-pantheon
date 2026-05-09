using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition.Schemas;

public sealed record LinkPositionInput(
    Id<Position> PositionId,
    Id<Position> TargetPositionId
);
