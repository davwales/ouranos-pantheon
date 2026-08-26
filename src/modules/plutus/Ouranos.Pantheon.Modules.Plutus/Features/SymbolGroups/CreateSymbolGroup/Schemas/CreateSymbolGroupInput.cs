using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup.Schemas;

public sealed record CreateSymbolGroupInput(Id<Market> MarketId, string Name, string? Description);
