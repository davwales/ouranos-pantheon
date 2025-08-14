using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application.Dtos;

public sealed record ItemDto(
    string SymbolCode,
    bool IsHighQuality,
    string SymbolName,
    AdditionalFields AdditionalFields
);