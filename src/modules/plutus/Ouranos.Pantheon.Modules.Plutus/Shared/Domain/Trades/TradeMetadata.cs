using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

public sealed record TradeMetadata(
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string SymbolName,
    string SymbolCode,
    string? SymbolSubcode,
    AdditionalFields AdditionalFields
);