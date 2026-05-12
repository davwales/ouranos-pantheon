using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup.Schemas;

public sealed record GetSymbolGroupResponse(
    Id<SymbolGroup> Id,
    Id<Market> MarketId,
    string Name,
    string? Description,
    IReadOnlyList<SymbolGroupSymbolResponse> Symbols
);
