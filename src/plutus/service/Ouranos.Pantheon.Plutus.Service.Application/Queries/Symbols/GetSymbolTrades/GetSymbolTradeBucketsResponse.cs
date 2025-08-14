namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Symbols.GetSymbolTrades;

public sealed record GetSymbolTradeBucketsResponse(
    decimal Price,
    decimal Volume,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    int NumTransactions,
    DateTimeOffset Date
);