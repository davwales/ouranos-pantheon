using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using TickerQ.Utilities.Base;
using MlForecastPoint = Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos.ForecastPoint;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator;

public sealed class ForecastGeneratorJob
{
    private readonly ILogger<ForecastGeneratorJob> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly IOuranosMachineLearningClient _mlClient;
    private readonly IOptions<PlutusOptions> _options;

    public ForecastGeneratorJob(
        ILogger<ForecastGeneratorJob> logger,
        PlutusDbContext dbContext,
        IOuranosMachineLearningClient mlClient,
        IOptions<PlutusOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(mlClient);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContext = dbContext;
        _mlClient = mlClient;
        _options = options;
    }

    [TickerFunction("ForecastGenerator", "0 0 0 * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken ct)
    {
        if (!_options.Value.Forecasting.IsEnabled)
        {
            _logger.LogInformation("Forecasting is disabled. Skipping execution.");
            return;
        }

        var symbols = await LoadForecastableSymbolsAsync(ct);
        if (symbols.Count == 0)
        {
            _logger.LogInformation("No symbols to process. Skipping execution.");
            return;
        }

        if (_options.Value.Forecasting.RemoveOutdatedForecasts)
        {
            _logger.LogInformation("Removing all outdated forecasts.");
            await _dbContext.Forecasts.ExecuteDeleteAsync(ct);
        }

        var totalForecasts = 0;

        foreach (var batch in symbols.Batch(_options.Value.Forecasting.BatchSize))
        {
            var batchList = batch.ToList();
            var inputs = await LoadBatchInputsAsync(batchList, ct);

            if (inputs.Count == 0)
            {
                continue;
            }

            var predictions = await _mlClient.GetPlutusForecasts(
                new GetPlutusForecastsRequest(_options.Value.Forecasting.NumPredictions, [.. inputs.Select(i => i.HistoricalPoints)]),
                ct
            );

            var forecasts = inputs
                .Select((input, i) =>
                    {
                        var latestMlPoint = input.HistoricalPoints[^1];

                        return Forecast.Create(
                            DatabaseExtensions.CreateId<Forecast>(),
                            input.Symbol.MarketId,
                            input.Symbol.Id,
                            latest: new ForecastPoint(
                                latestMlPoint.AveragePrice,
                                latestMlPoint.MinPrice,
                                latestMlPoint.MaxPrice,
                                latestMlPoint.Volume
                            ),
                            predictions:
                            [
                                .. predictions[i]
                                    .Select(p => new ForecastPoint(
                                            p.AveragePrice,
                                            p.MinPrice,
                                            p.MaxPrice,
                                            p.Volume
                                        )
                                    )
                            ]
                        );
                    }
                )
                .ToList();

            if (!_options.Value.Forecasting.RemoveOutdatedForecasts)
            {
                var forecastSymbolIds = forecasts.Select(f => f.SymbolId).ToList();
                var outdated = await _dbContext.Forecasts
                    .Where(f => forecastSymbolIds.Contains(f.SymbolId))
                    .ToListAsync(ct);
                _dbContext.Forecasts.RemoveRange(outdated);
            }

            _dbContext.Forecasts.AddRange(forecasts);
            await _dbContext.SaveChangesAsync(ct);

            totalForecasts += forecasts.Count;
        }

        _logger.LogInformation("Generated {Count} forecasts.", totalForecasts);
    }

    private async Task<List<Symbol>> LoadForecastableSymbolsAsync(CancellationToken ct)
    {
        var forecastableMarketIds = await _dbContext.Markets.AsNoTracking()
            .Where(m => m.IsForecastingEnabled)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (forecastableMarketIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Symbols.AsNoTracking()
            .Where(s => forecastableMarketIds.Contains(s.MarketId))
            .ToListAsync(ct);
    }

    private async Task<List<SymbolForecastInput>> LoadBatchInputsAsync(
        List<Symbol> symbols,
        CancellationToken ct
    )
    {
        var historyDays = _options.Value.Forecasting.HistoryDays;
        var since = DateTimeOffset.UtcNow - TimeSpan.FromDays(historyDays);
        var symbolIds = symbols.Select(s => s.Id).ToHashSet();

        var bucketedTrades = await _dbContext.Trades.AsNoTracking()
            .Where(t => symbolIds.Contains(t.SymbolId) && t.Timestamp >= since)
            .GroupBy(t => new
                {
                    t.SymbolId,
                    Bucket = TimescaleDbFunctions.TimeBucket(TimeSpan.FromDays(1), t.Timestamp)
                }
            )
            .Select(g => new
                {
                    g.Key.SymbolId,
                    g.Key.Bucket,
                    Volume = g.Sum(t => t.Volume),
                    AveragePrice = g.Sum(t => t.Price * t.Volume) / g.Sum(t => t.Volume),
                    MinPrice = g.Min(t => t.Price),
                    MaxPrice = g.Max(t => t.Price)
                }
            )
            .ToListAsync(ct);

        var bucketsBySymbol = bucketedTrades
            .GroupBy(b => b.SymbolId)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.Bucket).ToList());

        var symbolDict = symbols.ToDictionary(s => s.Id);

        return [
            .. bucketsBySymbol
                .Where(kvp => kvp.Value.Count == historyDays)
                .Select(kvp => new SymbolForecastInput(
                        symbolDict[kvp.Key],
                        [.. kvp.Value.Select(b => new MlForecastPoint(b.AveragePrice, b.MinPrice, b.MaxPrice, b.Volume))]
                    )
                )
        ];
    }
}
