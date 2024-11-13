using Microsoft.Extensions.DependencyInjection;
using Talos.Olympus.Service.Plutus.Application.Interfaces.Trades;
using Talos.Olympus.Service.Plutus.Infra.Mongo.Trades;

namespace Talos.Olympus.Service.Plutus.Infra.Mongo;

public static class MongoExtensions
{
    public static void RegisterMongoBehaviors(this IServiceCollection services)
    {
        services.AddScoped<IBucketTrades, BucketTrades>();
    }
}