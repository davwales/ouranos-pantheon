using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

public class MarketOverviewBucket : BaseEntity<Id<MarketOverviewBucket>>
{
    private MarketOverviewBucket(Id<MarketOverviewBucket> id)
        : base(id) { }

    public Id<Market> MarketId { get; private set; }

    public TimeFrame TimeFrame { get; private set; }

    public DateTimeOffset BucketStart { get; private set; }

    public decimal AveragePrice { get; private set; }

    public decimal Volume { get; private set; }

    public decimal TotalSpent { get; private set; }

    public int NumTransactions { get; private set; }

    private Market? _market;

    public Market Market =>
        _market ?? throw new NavigationPropertyNotLoadedException<MarketOverviewBucket>();

    public static MarketOverviewBucket Create(
        Id<Market> marketId,
        TimeFrame timeFrame,
        DateTimeOffset bucketStart,
        decimal averagePrice,
        decimal volume,
        decimal totalSpent,
        int numTransactions,
        Market? market = null
    )
    {
        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        return new MarketOverviewBucket(DatabaseExtensions.CreateId<MarketOverviewBucket>())
        {
            MarketId = marketId,
            TimeFrame = timeFrame,
            BucketStart = bucketStart,
            AveragePrice = averagePrice,
            Volume = volume,
            TotalSpent = totalSpent,
            NumTransactions = numTransactions,
            _market = market,
        };
    }
}
