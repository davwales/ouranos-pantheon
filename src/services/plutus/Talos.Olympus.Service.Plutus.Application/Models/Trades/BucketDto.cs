using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Symbols;

namespace Talos.Olympus.Service.Plutus.Application.Models.Trades;

public sealed record BucketDto(
    Id<Symbol> SymbolId,
    DateTime Date,
    decimal TotalSpent,
    decimal Volume,
    decimal MinPrice,
    decimal MaxPrice,
    int NumTransactions,
    decimal Price,
    decimal Margin
);