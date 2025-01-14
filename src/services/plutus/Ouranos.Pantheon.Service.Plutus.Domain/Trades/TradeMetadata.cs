using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Trades;

public sealed record TradeMetadata(
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string SymbolName,
    string SymbolCode,
    string? SymbolSubCode,
    AdditionalFields AdditionalFields
);