using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Dtos;

public sealed record ItemDto(
    string SymbolCode,
    bool IsHighQuality,
    string SymbolName,
    AdditionalFields AdditionalFields
);