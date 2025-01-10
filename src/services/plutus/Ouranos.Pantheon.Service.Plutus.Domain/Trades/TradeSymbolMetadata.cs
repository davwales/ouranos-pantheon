using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Trades;

public sealed record TradeSymbolMetadata(
    Id<Symbol> Id,
    Id<Market> MarketId,
    string Name,
    string Code,
    string? Subcode,
    AdditionalFields AdditionalFields
);