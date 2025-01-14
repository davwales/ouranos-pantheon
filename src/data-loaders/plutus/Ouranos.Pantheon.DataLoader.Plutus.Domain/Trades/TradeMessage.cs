using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;

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