using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive.Schemas;

public sealed record SetStrategyActiveInput(Id<Strategy> StrategyId, bool IsActive);
