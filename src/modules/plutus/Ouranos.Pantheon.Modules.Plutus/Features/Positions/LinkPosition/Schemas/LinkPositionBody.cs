using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition.Schemas;

public sealed record LinkPositionBody(Id<Position> TargetPositionId);
