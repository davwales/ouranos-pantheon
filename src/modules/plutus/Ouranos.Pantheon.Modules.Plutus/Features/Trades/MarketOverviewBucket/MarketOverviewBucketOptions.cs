namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket;

public sealed record MarketOverviewBucketOptions(int NumBuckets)
{
    public const string SectionName = "MarketOverviewBucket";

    public MarketOverviewBucketOptions() : this(NumBuckets: 100)
    {
    }
}
