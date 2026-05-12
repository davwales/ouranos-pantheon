using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup.Schemas;

public sealed record DeleteSymbolGroupInput(Id<SymbolGroup> SymbolGroupId);
