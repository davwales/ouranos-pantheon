using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot.Schemas;

internal sealed record SymbolAggregate(
    Symbol Symbol,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume,
    int NumTx,
    decimal Limit
);
