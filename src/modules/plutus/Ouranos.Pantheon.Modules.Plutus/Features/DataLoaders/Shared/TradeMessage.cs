using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

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
