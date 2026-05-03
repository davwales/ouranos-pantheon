using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive.Schemas;

public sealed record SetStrategyActiveInput(Id<Strategy> StrategyId, bool IsActive);
