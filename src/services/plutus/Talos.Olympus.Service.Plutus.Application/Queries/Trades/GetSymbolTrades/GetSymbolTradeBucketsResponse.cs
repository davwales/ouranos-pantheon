namespace Talos.Olympus.Service.Plutus.Application.Queries.Trades.GetSymbolTrades;

public sealed record GetSymbolTradeBucketsResponse(
    decimal Price,
    decimal Volume,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Margin,
    int NumTransactions,
    DateTime Date
);