using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;

public sealed record GetAllSymbolsResponse(
    Id<Symbol> Id,
    string Code,
    string? Subcode,
    string Name,
    Id<Market> MarketId
);
