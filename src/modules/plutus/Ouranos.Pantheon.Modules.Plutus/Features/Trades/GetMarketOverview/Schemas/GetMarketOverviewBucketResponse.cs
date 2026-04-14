namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;

public sealed record GetMarketOverviewBucketResponse(
    decimal Price,
    decimal Volume,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    int NumTransactions,
    DateTimeOffset Date,
    decimal OpenPrice,
    decimal ClosePrice
);
