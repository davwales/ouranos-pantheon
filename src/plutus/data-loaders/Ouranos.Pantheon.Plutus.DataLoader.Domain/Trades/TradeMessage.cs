using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Domain.Trades;

public sealed record TradeMessage(
    Producer Producer,
    string SymbolCode,
    string? SymbolSubcode,
    string SymbolName,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp,
    AdditionalFields AdditionalFields
);