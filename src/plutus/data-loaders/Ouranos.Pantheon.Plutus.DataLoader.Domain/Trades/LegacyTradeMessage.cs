using Ouranos.Pantheon.Plutus.DataLoader.Domain;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

// ReSharper disable all

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