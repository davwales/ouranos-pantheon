using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Models.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Forecasts;

public sealed class BucketHistoricalData : IBucketHistoricalData
{
    private readonly ILogger<BucketHistoricalData> _logger;

    public BucketHistoricalData(ILogger<BucketHistoricalData> logger)
    {
        Guard.Against.Null(logger);
        _logger = logger;
    }

    public IQueryable<ForecastBucketDto> ApplyBucketing(IQueryable<Trade> query)
    {
        _logger.LogTrace("Attempting to apply bucketing to historical forecasting data.");

        var bucketedQuery = query
            .GroupBy(t => new
                {
                    t.SymbolId,
                    Bucket = t.CreatedAt.Date
                }
            )
            .Select(g => new ForecastBucketDto(
                    new ForecastBucketIdDto(g.Key.SymbolId, g.Key.Bucket),
                    g.Sum(x => x.Price * x.Volume),
                    g.Min(x => x.Price),
                    g.Max(x => x.Price),
                    g.Sum(x => x.Volume)
                )
            );

        _logger.LogDebug("Successfully applied bucketing to historical forecasting data.");
        return bucketedQuery;
    }
}