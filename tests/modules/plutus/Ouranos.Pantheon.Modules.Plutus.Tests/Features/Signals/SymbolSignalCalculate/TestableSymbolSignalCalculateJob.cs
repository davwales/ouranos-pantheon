using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.SymbolSignalCalculate;

/// <summary>
///     Test-only subclass of <see cref="SymbolSignalCalculateJob" /> that replaces the
///     raw-SQL <c>time_bucket</c> aggregation and the <c>ExecuteDeleteAsync</c> purge
///     with in-memory-safe stubs. The EF Core in-memory provider cannot execute raw
///     SQL or <c>ExecuteDeleteAsync</c>, so the production seams are overridden here.
///     This stub mirrors the server-side aggregation over the small set of trades
///     seeded by each test - it is NOT used in production and never materializes the
///     24h trade window in the running service.
/// </summary>
internal sealed class TestableSymbolSignalCalculateJob : SymbolSignalCalculateJob
{
    private readonly PlutusDbContext _dbContext;

    public TestableSymbolSignalCalculateJob(
        ILogger<SymbolSignalCalculateJob> logger,
        PlutusDbContext dbContext,
        IOptions<SignalOptions> options,
        IEnumerable<ISignalComputer> computers
    )
        : base(logger, dbContext, options, computers)
    {
        _dbContext = dbContext;
    }

    protected internal override async Task<List<SymbolBucketRow>> LoadSymbolBucketsAsync(
        DateTimeOffset since,
        TimeSpan bucketInterval,
        CancellationToken ct
    )
    {
        var rows = await _dbContext
            .Trades.AsNoTracking()
            .Where(t => t.Timestamp >= since)
            .GroupBy(t => new
            {
                t.SymbolId,
                BucketStart = TimescaleDbFunctions.TimeBucket(bucketInterval, t.Timestamp),
            })
            .Select(g => new SymbolBucketRow(
                Guid.Parse(g.Key.SymbolId.Value),
                g.Key.BucketStart,
                g.Sum(t => t.Volume) > 0
                    ? g.Sum(t => t.Price * t.Volume) / g.Sum(t => t.Volume)
                    : g.Average(t => t.Price),
                g.Min(t => t.Price),
                g.Max(t => t.Price),
                g.Sum(t => t.Volume)
            ))
            .ToListAsync(ct);

        return [.. rows.OrderBy(r => r.SymbolId).ThenBy(r => r.BucketStart)];
    }

    protected internal override async Task<int> PurgeOldSignalsAsync(
        DateTimeOffset cutoff,
        CancellationToken ct
    )
    {
        var purgeable = await _dbContext.Signals.Where(s => s.ComputedAt < cutoff).ToListAsync(ct);

        if (purgeable.Count == 0)
        {
            return 0;
        }

        _dbContext.Signals.RemoveRange(purgeable);
        await _dbContext.SaveChangesAsync(ct);
        return purgeable.Count;
    }
}
