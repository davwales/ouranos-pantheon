using Microsoft.Extensions.DependencyInjection;

namespace Talos.Olympus.Service.Aphrodite.Infra.Mongo;

public static class MongoExtensions
{
    public static IServiceCollection RegisterMongoBehaviors(this IServiceCollection services)
    {
        return services;
    }
}