using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Hermes.Service.Infra.Mongo;

public static class MongoExtensions
{
    public static IServiceCollection RegisterMongoBehaviors(this IServiceCollection services)
    {
        return services;
    }
}