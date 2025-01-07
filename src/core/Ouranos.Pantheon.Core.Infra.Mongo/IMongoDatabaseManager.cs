using MongoDB.Driver;

namespace Ouranos.Pantheon.Core.Infra.Mongo;

public interface IMongoDatabaseManager
{
    IMongoDatabase GetDatabase<T>() where T : class;
}