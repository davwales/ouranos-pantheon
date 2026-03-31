using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup.Schemas;

public sealed record GetSymbolGroupInput(
    Id<SymbolGroup> SymbolGroupId,
    TimeFrame TimeFrame = TimeFrame.OneDay
);
