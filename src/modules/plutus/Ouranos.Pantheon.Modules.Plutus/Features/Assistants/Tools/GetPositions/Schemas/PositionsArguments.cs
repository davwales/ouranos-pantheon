using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPositions.Schemas;

public sealed record PositionsArguments(PositionStatus? Status);
