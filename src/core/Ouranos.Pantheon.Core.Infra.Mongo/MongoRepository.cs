using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Ouranos.Pantheon.Core.Infra.Mongo;

public sealed class MongoRepository<T> : IMongoRepository<T> where T : class
{
    private readonly IMongoDatabase _mongoDatabase;

    public MongoRepository(IMongoDatabase mongoDatabase)
    {
        ArgumentNullException.ThrowIfNull(mongoDatabase);

        _mongoDatabase = mongoDatabase;
    }

    public IMongoCollection<T> GetCollection()
    {
        var collectionName = Pluralize(typeof(T).Name).ToLower();
        return _mongoDatabase.GetCollection<T>(collectionName);
    }

    public IMongoQueryable<T> AsQueryable()
    {
        return GetCollection().AsQueryable();
    }

    private static string Pluralize(string name)
    {
        return name.EndsWith('s') ? name : $"{name}s";
    }
}