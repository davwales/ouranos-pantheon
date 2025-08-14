using Ouranos.Pantheon.Plutus.Service.Application.Models.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;

public interface IBucketHistoricalData
{
    IQueryable<ForecastBucketDto> ApplyBucketing(IQueryable<Trade> query);
}