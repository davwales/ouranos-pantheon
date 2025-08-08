using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Models.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres.Forecasts;

public sealed class BucketHistoricalData : IBucketHistoricalData
{
    public IQueryable<ForecastBucketDto> ApplyBucketing(IQueryable<Trade> query)
    {
        // TODO: Make a real implementation
        return query.Select(x => new ForecastBucketDto(
                new ForecastBucketIdDto(x.SymbolId, x.CreatedAt.DateTime),
                x.Price,
                x.Price,
                x.Price,
                x.Volume
            )
        );
    }
}