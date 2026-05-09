using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition.Schemas;

public sealed record ClosePositionInput(
    Id<Position> PositionId,
    PositionStatus CloseStatus
);
