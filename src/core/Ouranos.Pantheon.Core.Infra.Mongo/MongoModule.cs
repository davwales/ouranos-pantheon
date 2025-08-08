using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

public static class MongoModule
{
    public static IServiceCollection AddCoreMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));

        services.TryAddSingleton<IMongoClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MongoOptions>>();
                var mongoClientSettings = MongoClientSettings.FromConnectionString(options.Value.ConnectionString);
                return new MongoClient(mongoClientSettings);
            }
        );

        services.TryAddSingleton<IMongoDatabaseManager, MongoDatabaseManager>();
        services.TryAddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));
        services.TryAddScoped<IQueryExecutor, QueryExecutor>();

        RegisterConventions();
        return services;
    }

    private static void RegisterConventions()
    {
        var conventions = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new CamelCaseElementNameConvention(),
            new IgnoreIfNullConvention(true)
        };

        ConventionRegistry.Register("Custom Conventions", conventions, _ => true);
        BsonSerializer.RegisterGenericSerializerDefinition(typeof(Id<>), typeof(IdSerializer<>));
        BsonSerializer.TryRegisterSerializer(typeof(DateTimeOffset), new DateTimeOffsetSerializer(BsonType.DateTime));
        BsonSerializer.TryRegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.TryRegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));
    }
}