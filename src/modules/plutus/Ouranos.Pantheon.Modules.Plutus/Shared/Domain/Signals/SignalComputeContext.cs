using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

public sealed record SignalComputeContext(
    Id<Symbol> SymbolId,
    Id<Market> MarketId,
    MarketRules Rules,
    decimal Limit,
    MarketTradeSnapshot? ShortSnapshot,
    MarketTradeSnapshot? MediumSnapshot,
    MarketTradeSnapshot? LongSnapshot,
    IReadOnlyList<PriceBucket> PriceBuckets
);
