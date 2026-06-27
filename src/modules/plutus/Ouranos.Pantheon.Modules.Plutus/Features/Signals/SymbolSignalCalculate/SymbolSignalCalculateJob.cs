using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Querying;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate;

public class SymbolSignalCalculateJob
{
    private readonly ILogger<SymbolSignalCalculateJob> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<SignalOptions> _options;
    private readonly IReadOnlyList<ISignalComputer> _computers;
    private int _executing;

    public SymbolSignalCalculateJob(
        ILogger<SymbolSignalCalculateJob> logger,
        PlutusDbContext dbContext,
        IOptions<SignalOptions> options,
        IEnumerable<ISignalComputer> computers
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContext = dbContext;
        _options = options;
        _computers = [.. computers];
    }

    [TickerFunction("SymbolSignalCalculate", "0 * * * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken ct)
    {
        if (Interlocked.Exchange(ref _executing, 1) == 1)
        {
            _logger.LogWarning("SymbolSignalCalculate already running; skipping tick.");
            return;
        }

        try
        {
            var shortSnapshots = await _dbContext
                .MarketTradeSnapshots.AsNoTracking()
                .Where(s => s.TimeFrame == _options.Value.ShortTimeFrame)
                .ToDictionaryAsync(s => s.SymbolId, ct);

            var mediumSnapshots = await _dbContext
                .MarketTradeSnapshots.AsNoTracking()
                .Where(s => s.TimeFrame == _options.Value.MediumTimeFrame)
                .ToDictionaryAsync(s => s.SymbolId, ct);

            var longSnapshots = await _dbContext
                .MarketTradeSnapshots.AsNoTracking()
                .Where(s => s.TimeFrame == _options.Value.LongTimeFrame)
                .ToDictionaryAsync(s => s.SymbolId, ct);

            var taxRates = await _dbContext
                .Markets.AsNoTracking()
                .ToDictionaryAsync(m => m.Id, m => m.Taxes.Flat?.Rate ?? 0m, ct);

            var symbols = await _dbContext.Symbols.AsNoTracking().ToListAsync(ct);

            var since = DateTimeOffset.UtcNow - TimeSpan.FromDays(1);
            var bucketInterval = TimeSpan.FromDays(1).Divide(_options.Value.BucketCount);
            var rows = await LoadSymbolBucketsAsync(since, bucketInterval, ct);
            var bucketsBySymbol = BuildBucketsBySymbol(rows);

            var signals = new List<Signal>();

            foreach (var symbol in symbols)
            {
                var taxRate = taxRates.GetValueOrDefault(symbol.MarketId, 0m);
                var limit = symbol.AdditionalFields.Limit ?? 0m;

                shortSnapshots.TryGetValue(symbol.Id, out var shortSnap);
                mediumSnapshots.TryGetValue(symbol.Id, out var primarySnap);
                longSnapshots.TryGetValue(symbol.Id, out var longSnap);
                bucketsBySymbol.TryGetValue(symbol.Id, out var buckets);

                var context = new SignalComputeContext(
                    symbol.Id,
                    symbol.MarketId,
                    taxRate,
                    limit,
                    shortSnap,
                    primarySnap,
                    longSnap,
                    buckets ?? []
                );

                foreach (var computer in _computers)
                {
                    var value = await computer.ComputeAsync(context, ct);
                    if (value is not null)
                    {
                        signals.Add(
                            Signal.Create(symbol.MarketId, symbol.Id, computer.Type, value.Value)
                        );
                    }
                }
            }

            _dbContext.Signals.AddRange(signals);
            await _dbContext.SaveChangesAsync(ct);

            var cutoff = DateTimeOffset.UtcNow.AddDays(-_options.Value.HistoryRetentionDays);
            var purged = await PurgeOldSignalsAsync(cutoff, ct);
            if (purged > 0)
            {
                _logger.LogInformation(
                    "Purged {Count} signals older than {Cutoff}",
                    purged,
                    cutoff
                );
            }

            _logger.LogInformation(
                "Computed {Count} signals for {SymbolCount} symbols.",
                signals.Count,
                symbols.Count
            );
        }
        finally
        {
            Interlocked.Exchange(ref _executing, 0);
        }
    }

    /// <summary>
    ///     Loads fixed-anchor volume-weighted price buckets per symbol via a single
    ///     server-side TimescaleDB <c>time_bucket</c> aggregation, avoiding the
    ///     many GB in-memory materialization of raw 24h trades. Buckets are anchored
    ///     to <paramref name="since" /> via the <c>time_bucket</c> origin argument.
    ///     Overridable so tests can supply an in-memory-safe stub without exercising
    ///     raw SQL (the EF Core in-memory provider cannot run <c>time_bucket</c>).
    /// </summary>
    protected internal virtual async Task<List<SymbolBucketRow>> LoadSymbolBucketsAsync(
        DateTimeOffset since,
        TimeSpan bucketInterval,
        CancellationToken ct
    )
    {
        var intervalLiteral = bucketInterval.ToTimescaleInterval();

        var command = RawSqlCommand
            .FromSql(
                $"""
                SELECT symbol_id,
                       time_bucket('{intervalLiteral}'::interval, "timestamp", @since) AS bucket_start,
                       CASE WHEN SUM(volume) > 0
                            THEN SUM(price * volume) / SUM(volume)
                            ELSE AVG(price)
                       END AS average_price,
                       MIN(price) AS min_price,
                       MAX(price) AS max_price,
                       SUM(volume) AS volume
                FROM plutus.trades
                WHERE "timestamp" >= @since
                GROUP BY symbol_id, time_bucket('{intervalLiteral}'::interval, "timestamp", @since)
                ORDER BY symbol_id, bucket_start
                """
            )
            .WithDateTimeOffset("@since", since);

        return await _dbContext.Database.ExecuteQueryAsync<SymbolBucketRow>(command, ct);
    }

    /// <summary>
    ///     Purges signals older than <paramref name="cutoff" /> with a server-side
    ///     <c>ExecuteDeleteAsync</c> (<c>DELETE ... WHERE computed_at &lt; @cutoff</c>),
    ///     returning the deleted row count. No entity materialization, no
    ///     ChangeTracker - avoids the many GB load-all-then-RemoveRange leak.
    ///     Overridable so tests can stub the purge against the in-memory provider,
    ///     which cannot execute <c>ExecuteDeleteAsync</c>.
    /// </summary>
    protected internal virtual async Task<int> PurgeOldSignalsAsync(
        DateTimeOffset cutoff,
        CancellationToken ct
    )
    {
        // Batched server-side delete to stay well under the command timeout even when the
        // Signals table has a large accumulated backlog. Each batch deletes up to
        // batchSize rows (a fast, indexed DELETE on computed_at); the loop is capped per
        // tick so a huge backlog drains across successive ticks instead of one long run.
        const int batchSize = 5000;
        const int maxBatchesPerTick = 100;
        var totalPurged = 0;

        for (var i = 0; i < maxBatchesPerTick && !ct.IsCancellationRequested; i++)
        {
            var deleted = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                DELETE FROM plutus.signals
                WHERE id IN (
                    SELECT id FROM plutus.signals
                    WHERE computed_at < {cutoff}
                    ORDER BY computed_at
                    LIMIT {batchSize}
                )
                """,
                ct
            );

            totalPurged += deleted;
            if (deleted < batchSize)
            {
                break;
            }
        }

        return totalPurged;
    }

    private static Dictionary<Id<Symbol>, List<PriceBucket>> BuildBucketsBySymbol(
        List<SymbolBucketRow> rows
    )
    {
        var bucketsBySymbol = new Dictionary<Id<Symbol>, List<PriceBucket>>();

        foreach (var row in rows)
        {
            var symbolId = new Id<Symbol>(row.SymbolId.ToString());

            if (!bucketsBySymbol.TryGetValue(symbolId, out var list))
            {
                list = [];
                bucketsBySymbol[symbolId] = list;
            }

            list.Add(
                new PriceBucket(
                    row.BucketStart,
                    row.AveragePrice,
                    row.MinPrice,
                    row.MaxPrice,
                    row.Volume
                )
            );
        }

        return bucketsBySymbol;
    }
}
