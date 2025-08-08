using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Application.Models.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres.Trades;

public sealed class BucketTrades : IBucketTrades
{
    public IQueryable<BucketDto> GetBucketedTradesQuery(
        IQueryable<Trade> query,
        int numBuckets,
        CancellationToken cancellationToken = default
    )
    {
        var minDate = query.Min(x => x.CreatedAt);
        var maxDate = query.Max(x => x.CreatedAt);

        var totalRange = maxDate - minDate;
        var bucketSize = TimeSpan.FromTicks(totalRange.Ticks / numBuckets);

        return BucketByTimeSpan(query, bucketSize, minDate);
    }

    private IQueryable<BucketDto> BucketByTimeSpan(
        IQueryable<Trade> source,
        TimeSpan bucketSize,
        DateTimeOffset startDate
    )
    {
        return source
            .GroupBy(item => new
                {
                    item.SymbolId,
                    Bucket = startDate.AddTicks(
                        (int)((item.CreatedAt - startDate).Ticks / bucketSize.Ticks) *
                        bucketSize.Ticks
                    )
                }
            )
            .Select(group => new BucketDto(
                    group.Key.SymbolId,
                    group.Key.Bucket,
                    group.Sum(x => x.Price * x.Volume),
                    group.Sum(x => x.Volume),
                    group.Min(x => x.Price),
                    group.Max(x => x.Price),
                    group.Count(),
                    group.Average(x => x.Price),
                    group.Max(x => x.Price) - group.Min(x => x.Price)
                )
            )
            .OrderBy(bucket => bucket.Date);
    }
}