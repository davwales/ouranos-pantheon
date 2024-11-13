using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;
using Talos.Olympus.Service.Plutus.Domain.Symbols;

namespace Talos.Olympus.Service.Plutus.Domain.Trades;

public sealed record TradeSymbolMetadata(
    Id<Symbol> Id,
    Id<Market> MarketId,
    string Name,
    string Code,
    string Subcode,
    AdditionalFields AdditionalFields
);