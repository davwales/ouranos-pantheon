using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

public sealed record SignalComputeContext(
    Id<Symbol> SymbolId,
    Id<Market> MarketId,
    decimal TaxRate,
    decimal Limit,
    MarketTradeSnapshot? ShortSnapshot,
    MarketTradeSnapshot? MediumSnapshot,
    MarketTradeSnapshot? LongSnapshot,
    IReadOnlyList<PriceBucket> PriceBuckets
);
