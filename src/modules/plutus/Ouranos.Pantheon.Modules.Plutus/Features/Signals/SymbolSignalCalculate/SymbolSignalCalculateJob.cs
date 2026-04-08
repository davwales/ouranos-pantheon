using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate;

public sealed class SymbolSignalCalculateJob
{
    private readonly ILogger<SymbolSignalCalculateJob> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<SignalOptions> _options;
    private readonly IReadOnlyList<ISignalComputer> _computers;

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
        var shortSnapshots = await _dbContext.MarketTradeSnapshots.AsNoTracking()
            .Where(s => s.TimeFrame == _options.Value.ShortTimeFrame)
            .ToDictionaryAsync(s => s.SymbolId, ct);

        var mediumSnapshots = await _dbContext.MarketTradeSnapshots.AsNoTracking()
            .Where(s => s.TimeFrame == _options.Value.MediumTimeFrame)
            .ToDictionaryAsync(s => s.SymbolId, ct);

        var longSnapshots = await _dbContext.MarketTradeSnapshots.AsNoTracking()
            .Where(s => s.TimeFrame == _options.Value.LongTimeFrame)
            .ToDictionaryAsync(s => s.SymbolId, ct);

        var taxRates = await _dbContext.Markets.AsNoTracking()
            .ToDictionaryAsync(m => m.Id, m => m.Taxes.Flat?.Rate ?? 0m, ct);

        var symbols = await _dbContext.Symbols.AsNoTracking().ToListAsync(ct);

        var since = DateTimeOffset.UtcNow - TimeSpan.FromDays(1);
        var recentTrades = await _dbContext.Trades.AsNoTracking()
            .Where(t => t.Timestamp >= since)
            .Select(t => new { t.SymbolId, t.Price, t.Volume, t.Timestamp })
            .ToListAsync(ct);

        var bucketsBySymbol = recentTrades
            .GroupBy(t => t.SymbolId)
            .ToDictionary(
                g => g.Key,
                g => ComputeBuckets(
                    g.Select(t => (t.Timestamp, t.Price, t.Volume)),
                    _options.Value.BucketCount
                )
            );

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
                    signals.Add(Signal.Create(symbol.MarketId, symbol.Id, computer.Type, value.Value));
                }
            }
        }

        var symbolIds = symbols.Select(s => s.Id).ToHashSet();
        var existing = await _dbContext.Signals.Where(s => symbolIds.Contains(s.SymbolId)).ToListAsync(ct);
        _dbContext.Signals.RemoveRange(existing);
        _dbContext.Signals.AddRange(signals);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Computed {Count} signals for {SymbolCount} symbols.",
            signals.Count,
            symbols.Count
        );
    }

    private static IReadOnlyList<PriceBucket> ComputeBuckets(
        IEnumerable<(DateTimeOffset Timestamp, decimal Price, decimal Volume)> trades,
        int bucketCount
    )
    {
        var sorted = trades.OrderBy(t => t.Timestamp).ToList();
        if (sorted.Count == 0)
        {
            return [];
        }

        var start = sorted[0].Timestamp;
        var end = sorted[^1].Timestamp;
        var duration = end - start;

        if (duration <= TimeSpan.Zero)
        {
            var vol = sorted.Sum(t => t.Volume);
            var avgPrice = vol > 0
                ? sorted.Sum(t => t.Price * t.Volume) / vol
                : sorted.Average(t => t.Price);

            return
            [
                new PriceBucket(
                    start,
                    avgPrice,
                    sorted.Min(t => t.Price),
                    sorted.Max(t => t.Price),
                    vol
                )
            ];
        }

        var bucketSize = duration.TotalSeconds / bucketCount;

        return
        [
            .. sorted
                .GroupBy(t => (int)((t.Timestamp - start).TotalSeconds / bucketSize))
                .Select(g =>
                    {
                        var vol = g.Sum(t => t.Volume);
                        var avgPrice = vol > 0
                            ? g.Sum(t => t.Price * t.Volume) / vol
                            : g.Average(t => t.Price);

                        return new PriceBucket(
                            start + TimeSpan.FromSeconds(g.Key * bucketSize),
                            avgPrice,
                            g.Min(t => t.Price),
                            g.Max(t => t.Price),
                            vol
                        );
                    }
                )
                .OrderBy(b => b.BucketStart)
        ];
    }
}
