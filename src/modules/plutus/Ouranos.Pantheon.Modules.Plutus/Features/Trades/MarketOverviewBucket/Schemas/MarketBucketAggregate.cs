namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket.Schemas;

internal sealed record MarketBucketAggregate(
    DateTimeOffset BucketStart,
    decimal TotalSpent,
    decimal Volume,
    int NumTransactions,
    decimal AveragePrice
);
