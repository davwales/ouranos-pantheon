namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket.Schemas;

internal sealed record MarketBucketAggregate(
    DateTimeOffset BucketStart,
    decimal TotalSpent,
    decimal Volume,
    decimal MinPrice,
    decimal MaxPrice,
    int NumTransactions,
    decimal AveragePrice,
    decimal OpenPrice,
    decimal ClosePrice
);
