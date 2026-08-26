using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;

public sealed record BucketDto(
    Id<Symbol> SymbolId,
    DateTimeOffset BucketStart,
    decimal TotalSpent,
    decimal Volume,
    decimal MinPrice,
    decimal MaxPrice,
    int NumTransactions,
    decimal AveragePrice,
    decimal Margin,
    decimal OpenPrice,
    decimal ClosePrice
);
