namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket;

public sealed record MarketOverviewBucketOptions(int NumBuckets, int ChunkDays, int ChunkThresholdDays)
{
    public const string SectionName = "MarketOverviewBucket";

    public MarketOverviewBucketOptions() : this(NumBuckets: 100, ChunkDays: 30, ChunkThresholdDays: 90)
    {
    }
}
