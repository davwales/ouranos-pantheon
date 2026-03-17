using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public sealed record ItemDto(
    string SymbolCode,
    bool IsHighQuality,
    string SymbolName,
    AdditionalFields AdditionalFields
);
