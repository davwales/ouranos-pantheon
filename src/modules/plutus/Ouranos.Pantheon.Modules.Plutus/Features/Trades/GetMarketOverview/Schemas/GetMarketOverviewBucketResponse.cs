namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;

public sealed record GetMarketOverviewBucketResponse(
    decimal Price,
    decimal Volume,
    decimal TotalSpent,
    int NumTransactions,
    DateTimeOffset Date
);
