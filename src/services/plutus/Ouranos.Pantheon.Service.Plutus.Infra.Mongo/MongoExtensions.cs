using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Infra.Mongo.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Mongo;

public static class MongoExtensions
{
    public static IServiceCollection RegisterMongoBehaviors(this IServiceCollection services)
    {
        return services.AddScoped<IBucketTrades, BucketTrades>();
    }
}