using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record OpenPosition(
    Id<Symbol> SymbolId,
    string SymbolName,
    string? SymbolSubcode,
    decimal EntryPrice,
    decimal Volume,
    DateTimeOffset EntryTime
);
