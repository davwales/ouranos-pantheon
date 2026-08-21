using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

public sealed record MarketTradeSnapshot(
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    TimeFrame TimeFrame,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume,
    int NumTransactions,
    decimal Limit,
    decimal Tax
);
