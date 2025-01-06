using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Service.Hermes.Infra.Mongo;

public static class MongoExtensions
{
    public static IServiceCollection RegisterMongoBehaviors(this IServiceCollection services)
    {
        return services;
    }
}