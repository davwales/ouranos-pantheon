using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastEvaluator;

public sealed class ForecastEvaluatorJob
{
    private readonly ILogger<ForecastEvaluatorJob> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<PlutusOptions> _options;

    public ForecastEvaluatorJob(
        ILogger<ForecastEvaluatorJob> logger,
        PlutusDbContext dbContext,
        IOptions<PlutusOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContext = dbContext;
        _options = options;
    }

    [TickerFunction("ForecastEvaluator", "0 30 0 * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken ct)
    {
        if (!_options.Value.Forecasting.IsEnabled)
        {
            _logger.LogInformation("Forecasting is disabled. Skipping evaluation.");
            return;
        }

        var cutoff = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date, TimeSpan.Zero);
        var oldestTarget = cutoff.AddDays(-_options.Value.Forecasting.MaxEvaluationAgeDays);

        var pending = await _dbContext.ForecastRecords
            .Where(r => r.TargetAt < cutoff && r.TargetAt >= oldestTarget && r.EvaluatedAt == null)
            .ToListAsync(ct);

        if (pending.Count == 0)
        {
            _logger.LogInformation("No forecast records pending evaluation.");
            return;
        }

        var totalEvaluated = 0;
        var totalSkipped = 0;

        foreach (var batch in pending.Batch(_options.Value.Forecasting.BatchSize))
        {
            var batchList = batch.ToList();

            var symbolIds = batchList.Select(r => r.SymbolId).ToHashSet();
            var minTarget = batchList.Min(r => r.TargetAt);
            var maxTarget = batchList.Max(r => r.TargetAt);

            var actuals = await _dbContext.Trades.AsNoTracking()
                .Where(t =>
                    symbolIds.Contains(t.SymbolId) &&
                    t.Timestamp >= minTarget &&
                    t.Timestamp < maxTarget.AddDays(1)
                )
                .GroupBy(t => new
                {
                    t.SymbolId,
                    Bucket = TimescaleDbFunctions.TimeBucket(TimeSpan.FromDays(1), t.Timestamp)
                })
                .Select(g => new
                {
                    g.Key.SymbolId,
                    g.Key.Bucket,
                    AveragePrice = g.Sum(t => t.Price * t.Volume) / g.Sum(t => t.Volume),
                    MinPrice = g.Min(t => t.Price),
                    MaxPrice = g.Max(t => t.Price),
                    Volume = g.Sum(t => t.Volume)
                })
                .ToListAsync(ct);

            var actualsByKey = actuals.ToDictionary(
                a => (a.SymbolId, a.Bucket),
                a => new ForecastPoint(a.AveragePrice, a.MinPrice, a.MaxPrice, a.Volume)
            );

            var evaluatedAt = DateTimeOffset.UtcNow;

            foreach (var record in batchList)
            {
                if (actualsByKey.TryGetValue((record.SymbolId, record.TargetAt), out var actual))
                {
                    record.RecordActual(actual, evaluatedAt);
                    totalEvaluated++;
                }
                else
                {
                    totalSkipped++;
                }
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "Forecast evaluation complete. Evaluated: {Evaluated}, Skipped (no trades): {Skipped}.",
            totalEvaluated,
            totalSkipped
        );
    }
}
