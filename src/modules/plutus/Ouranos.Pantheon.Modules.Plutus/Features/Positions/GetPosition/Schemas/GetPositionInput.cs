using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition.Schemas;

public sealed record GetPositionInput(Id<Position> PositionId);
