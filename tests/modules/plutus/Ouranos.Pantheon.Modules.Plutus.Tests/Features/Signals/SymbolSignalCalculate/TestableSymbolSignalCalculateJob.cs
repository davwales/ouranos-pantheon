using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Functions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.SymbolSignalCalculate;

/// <summary>
///     Test-only subclass of <see cref="SymbolSignalCalculateJob" /> that replaces the
///     raw-SQL <c>time_bucket</c> aggregation with an in-memory-safe stub. The EF Core
///     in-memory provider cannot execute raw SQL, so the production seam is overridden
///     here. The manual signal purge (previously overridden here too) has been removed
///     entirely - retention is now enforced by TimescaleDB's
///     <c>add_retention_policy</c> background job, declared in the
///     <c>ConvertSignalsToHypertable</c> EF migration.
/// </summary>
internal sealed class TestableSymbolSignalCalculateJob(
    ILogger<SymbolSignalCalculateJob> logger,
    PlutusDbContext dbContext,
    IOptions<SignalOptions> options,
    IEnumerable<ISignalComputer> computers
) : SymbolSignalCalculateJob(logger, dbContext, options, computers)
{
    private readonly PlutusDbContext _dbContext = dbContext;

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
}
