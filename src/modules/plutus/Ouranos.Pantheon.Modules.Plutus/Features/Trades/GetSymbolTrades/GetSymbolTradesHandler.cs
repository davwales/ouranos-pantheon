using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;

internal sealed record BucketOpenClose(DateTimeOffset BucketStart, decimal OpenPrice, decimal ClosePrice);

public sealed class GetSymbolTradesHandler : IPantheonHandler<GetSymbolTradesInput, GetSymbolTradesResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetSymbolTradesHandler> _logger;

    public GetSymbolTradesHandler(
        ILogger<GetSymbolTradesHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetSymbolTradesResponse> Handle(
        GetSymbolTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbol trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset? since = query.TimeFrame.ToTimeSpan() is { } span
            ? DateTimeOffset.UtcNow - span
            : null;

        var baseQuery = _dbContext.Trades
            .Where(t => t.SymbolId == query.SymbolId && (since == null || t.Timestamp >= since));

        var aggregatedStats = await baseQuery
            .GroupBy(t => 1)
            .Select(g => new
            {
                MinPrice = g.Min(t => t.Price),
                MaxPrice = g.Max(t => t.Price),
                TotalSpent = g.Sum(t => t.Price * t.Volume),
                Volume = g.Sum(t => t.Volume),
                NumTransactions = g.Count()
            }
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (aggregatedStats is null)
        {
            _logger.LogDebug("No trades found for symbol '{symbolId}'.", query.SymbolId);
            return new GetSymbolTradesResponse(0, 0, 0, 0, 0, 0, []);
        }

        var (buckets, interval) = await GetBucketedTrades(baseQuery, query.NumBuckets, cancellationToken);

        if (buckets.Count > 0 && interval.HasValue)
        {
            try
            {
                var openClose = await GetOpenCloseAsync(
                    _dbContext,
                    query.SymbolId,
                    since,
                    interval.Value,
                    cancellationToken
                );

                buckets =
                [
                    .. buckets.Select(b =>
                        {
                            if (!openClose.TryGetValue(b.BucketStart, out var oc))
                            {
                                return b;
                            }

                            return b with { OpenPrice = oc.OpenPrice, ClosePrice = oc.ClosePrice };
                        }
                    )
                ];
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("relational"))
            {
                _logger.LogWarning("Open/close prices unavailable: database is not relational.");
            }
        }

        var response = new GetSymbolTradesResponse(
            aggregatedStats.MinPrice,
            aggregatedStats.MaxPrice,
            aggregatedStats.Volume > 0
                ? aggregatedStats.TotalSpent / aggregatedStats.Volume
                : 0m,
            aggregatedStats.TotalSpent,
            aggregatedStats.Volume,
            aggregatedStats.NumTransactions,
            [
                .. buckets
                    .Select(b => new GetSymbolTradeBucketsResponse(
                            b.AveragePrice,
                            b.Volume,
                            b.TotalSpent,
                            b.MinPrice,
                            b.MaxPrice,
                            b.NumTransactions,
                            b.BucketStart,
                            b.OpenPrice,
                            b.ClosePrice
                        )
                    )
            ]
        );

        _logger.LogDebug("Successfully handled get symbol trades request.");
        return response;
    }

    private static async Task<Dictionary<DateTimeOffset, BucketOpenClose>> GetOpenCloseAsync(
        PlutusDbContext dbContext,
        Id<Symbol> symbolId,
        DateTimeOffset? since,
        TimeSpan interval,
        CancellationToken cancellationToken
    )
    {
        var intervalLiteral = interval.ToTimescaleInterval();

        string sql;
        object[] parameters;

        if (since is not null)
        {
            sql = $"""
                   SELECT time_bucket('{intervalLiteral}'::interval, "timestamp") AS bucket_start,
                          first(price, "timestamp") AS open_price,
                          last(price, "timestamp") AS close_price
                   FROM plutus.trades
                   WHERE symbol_id = @symbolId AND "timestamp" >= @since
                   GROUP BY bucket_start
                   """;
            parameters = [new NpgsqlParameter("@symbolId", symbolId.Value), new NpgsqlParameter("@since", since.Value)];
        }
        else
        {
            sql = $"""
                   SELECT time_bucket('{intervalLiteral}'::interval, "timestamp") AS bucket_start,
                          first(price, "timestamp") AS open_price,
                          last(price, "timestamp") AS close_price
                   FROM plutus.trades
                   WHERE symbol_id = @symbolId
                   GROUP BY bucket_start
                   """;
            parameters = [new NpgsqlParameter("@symbolId", symbolId.Value)];
        }

        var results = await dbContext.Database
            .SqlQueryRaw<BucketOpenClose>(sql, parameters)
            .ToListAsync(cancellationToken);

        return results.ToDictionary(r => r.BucketStart);
    }

    private static async Task<(List<BucketDto> Buckets, TimeSpan? Interval)> GetBucketedTrades(
        IQueryable<Trade> query,
        int numBuckets,
        CancellationToken cancellationToken
    )
    {
        var timeRange = await query
            .GroupBy(t => 1)
            .Select(g => new
            {
                StartTime = g.Min(t => t.Timestamp),
                EndTime = g.Max(t => t.Timestamp),
                Duration = g.Max(t => t.Timestamp) - g.Min(t => t.Timestamp)
            }
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (timeRange is null || timeRange.Duration <= TimeSpan.Zero)
        {
            return ([], null);
        }

        var interval = SmartIntervalCalculator.Calculate(timeRange.Duration, numBuckets);

        var buckets = await query
            .GroupBy(t => TimescaleDbFunctions.TimeBucket(interval, t.Timestamp))
            .Select(group => new BucketDto(
                    group.First().SymbolId,
                    group.Key,
                    group.Sum(x => x.Price * x.Volume),
                    group.Sum(x => x.Volume),
                    group.Min(x => x.Price),
                    group.Max(x => x.Price),
                    group.Count(),
                    group.Sum(x => x.Price * x.Volume) / group.Sum(x => x.Volume),
                    group.Max(x => x.Price) - group.Min(x => x.Price),
                    0m,
                    0m
                )
            )
            .ToListAsync(cancellationToken);

        return ([.. buckets.OrderBy(b => b.BucketStart)], interval);
    }
}
