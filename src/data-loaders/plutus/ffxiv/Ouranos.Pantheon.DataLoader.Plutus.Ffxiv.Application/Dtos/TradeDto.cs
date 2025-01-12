namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Dtos;

public sealed record TradeDto(
    string SymbolCode,
    bool IsHighQuality,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp
);