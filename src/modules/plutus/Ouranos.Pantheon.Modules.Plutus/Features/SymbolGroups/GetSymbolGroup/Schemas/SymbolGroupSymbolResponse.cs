using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup.Schemas;

public sealed record SymbolGroupSymbolResponse(
    Id<Symbol> SymbolId,
    string Code,
    string? Subcode,
    string Name,
    DateTimeOffset AddedAt,
    decimal? Volume,
    decimal? Gain,
    decimal? Roi,
    decimal? SignalScore
);
