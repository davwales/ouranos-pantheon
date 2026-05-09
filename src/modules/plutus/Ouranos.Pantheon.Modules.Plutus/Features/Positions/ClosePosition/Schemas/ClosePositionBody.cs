using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition.Schemas;

public sealed record ClosePositionBody(
    PositionStatus CloseStatus
);
