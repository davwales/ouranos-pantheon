using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Models.Trades;

public sealed record BucketDto(
    Id<Symbol> SymbolId,
    DateTimeOffset Date,
    decimal TotalSpent,
    decimal Volume,
    decimal MinPrice,
    decimal MaxPrice,
    int NumTransactions,
    decimal Price,
    decimal Margin
);