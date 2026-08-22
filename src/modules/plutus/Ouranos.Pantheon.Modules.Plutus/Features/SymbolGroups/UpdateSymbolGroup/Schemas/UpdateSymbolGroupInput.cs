using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup.Schemas;

public sealed record UpdateSymbolGroupInput(
    Id<SymbolGroup> SymbolGroupId,
    string Name,
    string? Description,
    ICollection<Id<Symbol>> SymbolIds
);
