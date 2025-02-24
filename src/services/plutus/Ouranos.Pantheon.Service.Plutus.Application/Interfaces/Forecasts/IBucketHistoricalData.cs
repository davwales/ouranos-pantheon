using Ouranos.Pantheon.Service.Plutus.Application.Models.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;

public interface IBucketHistoricalData
{
    IQueryable<ForecastBucketDto> ApplyBucketing(IQueryable<Trade> query);
}