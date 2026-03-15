namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;

public sealed record GetSymbolTradeBucketsResponse(
    decimal Price,
    decimal Volume,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    int NumTransactions,
    DateTimeOffset Date
);
