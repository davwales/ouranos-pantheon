using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;
using TickerQ.Utilities.Base;
using MlForecastPoint = Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos.ForecastPoint;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator;

public sealed class ForecastGeneratorJob
{
    private const int NumPredictions = 7;
    private const int HistoryDays = 30;

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

        var inputs = await LoadInputsAsync(ct);
        if (inputs.Count == 0)
        {
            _logger.LogInformation("No inputs to process. Skipping execution.");
            return;
        }

        var predictions = await _mlClient.GetPlutusForecasts(
            new GetPlutusForecastsRequest(NumPredictions, [.. inputs.Select(i => i.HistoricalPoints)]),
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

        _dbContext.Forecasts.AddRange(forecasts);

        await RemoveOutdatedForecasts(forecasts, ct);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Generated {Count} forecasts.", forecasts.Count);
    }

    private async Task<List<SymbolForecastInput>> LoadInputsAsync(CancellationToken ct)
    {
        var since = DateTimeOffset.UtcNow - TimeSpan.FromDays(HistoryDays);

        var forecastableMarketIds = await _dbContext.Markets.AsNoTracking()
            .Where(m => m.IsForecastingEnabled)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (forecastableMarketIds.Count == 0)
        {
            return [];
        }

        var symbols = await _dbContext.Symbols.AsNoTracking()
            .Where(s => forecastableMarketIds.Contains(s.MarketId))
            .ToListAsync(ct);

        var symbolIds = symbols.Select(s => s.Id).ToHashSet();

        var trades = await _dbContext.Trades.AsNoTracking()
            .Where(t => symbolIds.Contains(t.SymbolId) && t.Timestamp >= since)
            .Select(t => new { t.SymbolId, t.Price, t.Volume, t.Timestamp })
            .ToListAsync(ct);

        var tradesBySymbol = trades
            .GroupBy(t => t.SymbolId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var buckets = symbols
            .Where(s => tradesBySymbol.ContainsKey(s.Id))
            .Select(s =>
                new SymbolForecastInput(
                    s,
                    BuildDailyBuckets(tradesBySymbol[s.Id].Select(t => (t.Timestamp, t.Price, t.Volume)))
                )
            )
            .Where(b => b.HistoricalPoints.Count == HistoryDays)
            .ToList();

        return buckets;
    }

    private async Task RemoveOutdatedForecasts(List<Forecast> newForecasts, CancellationToken ct)
    {
        if (_options.Value.Forecasting.RemoveOutdatedForecasts)
        {
            _logger.LogInformation("Removing all outdated forecasts.");
            await _dbContext.Forecasts.ExecuteDeleteAsync(ct);
            return;
        }

        var newSymbolForecasts = await _dbContext.Forecasts
            .Where(f => newForecasts.Select(x => x.SymbolId).Contains(f.SymbolId))
            .ToListAsync(ct);

        _dbContext.Forecasts.RemoveRange(newSymbolForecasts);
    }

    private static List<MlForecastPoint> BuildDailyBuckets(
        IEnumerable<(DateTimeOffset Timestamp, decimal Price, decimal Volume)> trades
    )
    {
        return
        [
            .. trades
                .GroupBy(t => t.Timestamp.Date)
                .OrderBy(g => g.Key)
                .Select(g =>
                    {
                        var vol = g.Sum(t => t.Volume);
                        var avgPrice = vol > 0
                            ? g.Sum(t => t.Price * t.Volume) / vol
                            : g.Average(t => t.Price);

                        return new MlForecastPoint(
                            avgPrice,
                            g.Min(t => t.Price),
                            g.Max(t => t.Price),
                            vol
                        );
                    }
                )
        ];
    }
}
