using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups.Schemas;

internal sealed record SnapshotData(
    Id<Symbol> SymbolId,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume,
    decimal Limit,
    decimal Tax
);
