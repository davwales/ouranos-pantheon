using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Talos.Olympus.Core.Infra.Mongo;

public interface IMongoRepository<T> where T : class
{
    IMongoCollection<T> GetCollection();

    IMongoQueryable<T> AsQueryable();
}