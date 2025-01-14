using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Infra.Mongo.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Mongo;

public static class MongoModule
{
    public static IServiceCollection AddMongoModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCoreMongo(configuration)
            .AddScoped<IBucketTrades, BucketTrades>();
    }
}