using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Core.Infra.Mongo.Common;
using Talos.Olympus.Core.Infra.Mongo.Serializers;

namespace Talos.Olympus.Core.Infra.Mongo;

public static class MongoExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration.GetValue<string?>("MONGO_URL");

        services.AddSingleton<IMongoClient>(_ =>
        {
            var mongoClientSettings = MongoClientSettings.FromConnectionString(url);
            return new MongoClient(mongoClientSettings);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var mongoUrl = new MongoUrl(url);
            return sp.GetRequiredService<IMongoClient>().GetDatabase(mongoUrl.DatabaseName);
        });

        services.AddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));
        services.AddScoped<IQueryExecutor, QueryExecutor>();

        RegisterInfrastructureBehaviors(services);
        RegisterConventions();
        return services;
    }

    private static void RegisterInfrastructureBehaviors(IServiceCollection services)
    {
        services.AddScoped(typeof(ICrudRepository<>), typeof(CrudRepository<>));
        services.AddScoped(typeof(ICreateDatabaseId<>), typeof(CreateDatabaseId<>));
    }

    private static void RegisterConventions()
    {
        var conventions = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new CamelCaseElementNameConvention()
        };

        ConventionRegistry.Register("Custom Conventions", conventions, _ => true);
        BsonSerializer.RegisterGenericSerializerDefinition(typeof(Id<>), typeof(IdSerializer<>));
        BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
    }
}