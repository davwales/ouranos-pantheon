namespace Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;

public sealed record TradeMessage(
    Producer Producer,
    string SymbolCode,
    string? SymbolSubCode,
    string SymbolName,
    long? Limit,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp,
    Dictionary<string, object?> AdditionalFields
);