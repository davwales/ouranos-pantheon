using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Mongo.Common;
using Ouranos.Pantheon.Core.Infra.Mongo.Serializers;

namespace Ouranos.Pantheon.Core.Infra.Mongo;

public static class MongoExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        
        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>();
            var mongoClientSettings = MongoClientSettings.FromConnectionString(options.Value.ConnectionString);
            return new MongoClient(mongoClientSettings);
        });

        services.AddSingleton<IMongoDatabaseManager, MongoDatabaseManager>();
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