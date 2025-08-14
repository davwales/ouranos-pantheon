using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Trades;
using Ouranos.Pantheon.Plutus.Service.Infra.Mongo.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Infra.Mongo.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Mongo;

public static class MongoModule
{
    public static IServiceCollection AddMongoModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCoreMongo(configuration)
            .AddScoped<IBucketTrades, BucketTrades>()
            .AddScoped<IBucketHistoricalData, BucketHistoricalData>();
    }
}