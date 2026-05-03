using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

internal sealed record OpenPosition(
    Id<Symbol> SymbolId,
    string SymbolName,
    string? SymbolSubcode,
    decimal EntryPrice,
    decimal Volume,
    DateTimeOffset EntryTime
);
