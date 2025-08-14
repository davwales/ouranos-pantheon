using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Infra.Postgres.Functions;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Application.Models.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres.Trades;

public sealed class BucketTrades : IBucketTrades
{
    private readonly ILogger<BucketTrades> _logger;

    public BucketTrades(ILogger<BucketTrades> logger)
    {
        Guard.Against.Null(logger);
        _logger = logger;
    }

    public IQueryable<BucketDto> GetBucketedTradesQuery(
        IQueryable<Trade> query,
        int numBuckets,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to bucket trades for a Postgres query.");
        cancellationToken.ThrowIfCancellationRequested();

        var timeRange = query
            .GroupBy(t => 1)
            .Select(g => new
                {
                    StartTime = g.Min(t => t.CreatedAt),
                    EndTime = g.Max(t => t.CreatedAt),
                    Duration = g.Max(t => t.CreatedAt) - g.Min(t => t.CreatedAt)
                }
            )
            .FirstOrDefault();

        if (timeRange is null)
        {
            return Enumerable.Empty<BucketDto>().AsQueryable();
        }

        var interval = CalculateSmartInterval(timeRange.Duration);
        var bucketedQuery = query
            .GroupBy(t => TimescaleDbFunctions.TimeBucket(interval, t.CreatedAt))
            .Select(group => new BucketDto(
                    group.First().SymbolId,
                    group.Key,
                    group.Sum(x => x.Price * x.Volume),
                    group.Sum(x => x.Volume),
                    group.Min(x => x.Price),
                    group.Max(x => x.Price),
                    group.Count(),
                    group.Sum(x => x.Price * x.Volume) / group.Sum(x => x.Volume),
                    group.Max(x => x.Price) - group.Min(x => x.Price)
                )
            );

        _logger.LogDebug("Successfully bucketed trades for a Postgres query.");
        return bucketedQuery;
    }

    private static TimeSpan CalculateSmartInterval(TimeSpan duration)
    {
        return duration.TotalSeconds switch
        {
            <= 3600 => TimeSpan.FromMinutes(5), // ≤1 hour -> 5min buckets
            <= 86400 => TimeSpan.FromMinutes(30), // ≤1 day -> 30min buckets  
            <= 604800 => TimeSpan.FromHours(2), // ≤1 week -> 2hour buckets
            <= 2592000 => TimeSpan.FromHours(12), // ≤1 month -> 12hour buckets
            <= 31536000 => TimeSpan.FromDays(1), // ≤1 year -> 1day buckets
            _ => TimeSpan.FromDays(7) // >1 year -> 1week buckets
        };
    }
}