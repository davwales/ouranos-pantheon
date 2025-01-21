using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ouranos.Pantheon.Core.Infra.Mongo;

public sealed class MongoDatabaseManager : IMongoDatabaseManager
{
    private const string DefaultDatabase = "ouranos";
    private readonly Dictionary<Assembly, string> _assemblyMappings = [];
    private readonly IMongoClient _client;
    private readonly Dictionary<Type, IMongoDatabase> _databases = [];
    private readonly MongoOptions _options;
    private readonly Dictionary<Type, string> _typeMappings = [];

    public MongoDatabaseManager(
        IMongoClient client,
        IOptions<MongoOptions> options
    )
    {
        Guard.Against.Null(client);
        Guard.Against.Null(options);
        Guard.Against.Null(options.Value);

        _client = client;
        _options = options.Value;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        SetupAssemblyMappings(assemblies);
        SetupTypeMappings(assemblies);
    }

    public IMongoDatabase GetDatabase<T>() where T : class
    {
        var type = typeof(T);
        if (_databases.TryGetValue(type, out var database))
        {
            return database;
        }

        database = CreateDatabaseReference(type);
        _databases[type] = database;
        return database;
    }

    private IMongoDatabase CreateDatabaseReference(Type type)
    {
        if (_typeMappings.TryGetValue(type, out var typeDatabase))
        {
            return _client.GetDatabase(typeDatabase);
        }

        if (_assemblyMappings.TryGetValue(type.Assembly, out var assemblyDatabase))
        {
            return _client.GetDatabase(assemblyDatabase);
        }

        return _client.GetDatabase(DefaultDatabase);
    }

    private void SetupAssemblyMappings(Assembly[] assemblies)
    {
        foreach (var (assemblyName, dbName) in _options.AssemblyDatabases)
        {
            var assembly = assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly is not null)
            {
                _assemblyMappings[assembly] = dbName;
            }
        }
    }

    private void SetupTypeMappings(Assembly[] assemblies)
    {
        var assemblyTypes = assemblies.SelectMany(a => a.GetTypes()).ToList();
        foreach (var (typeName, dbName) in _options.TypeDatabases)
        {
            var type = assemblyTypes.FirstOrDefault(t => t.FullName == typeName);

            if (type is not null)
            {
                _typeMappings[type] = dbName;
            }
        }
    }
}