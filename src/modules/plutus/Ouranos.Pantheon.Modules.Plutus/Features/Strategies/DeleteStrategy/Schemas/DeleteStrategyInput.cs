using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy.Schemas;

public sealed record DeleteStrategyInput(Id<Strategy> StrategyId);
