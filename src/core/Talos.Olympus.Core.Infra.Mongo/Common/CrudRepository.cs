using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Infra.Mongo.Common;

public sealed class CrudRepository<T> : ICrudRepository<T> where T : BaseEntity<Id<T>>
{
    private readonly ILogger<CrudRepository<T>> _logger;
    private readonly IMongoRepository<T> _mongoRepository;

    public CrudRepository(ILogger<CrudRepository<T>> logger, IMongoRepository<T> mongoRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mongoRepository);

        _logger = logger;
        _mongoRepository = mongoRepository;
    }

    public async Task Create(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to create {type} '{@entity}'.", typeof(T).Name, entity);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        await collection.InsertOneAsync(entity, default, cancellationToken);

        _logger.LogDebug("Successfully created {type} '{id}'.", typeof(T).Name, entity.Id);
    }

    public async Task<T> Read(Id<T> id, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to read {type} '{id}' in Mongo.", typeof(T).Name, id);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var entity = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
                     ?? throw new KeyNotFoundException($"Failed to read {typeof(T).Name} '{id}' in Mongo.");

        _logger.LogDebug("Successfully read {type} '{id}' in Mongo.", typeof(T).Name, id);
        return entity;
    }

    public async Task<IEnumerable<T>> ReadAll(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to read all {type}.", typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        var filter = Builders<T>.Filter.Empty;
        var entities = await collection.Find(filter).ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully read {count} entities from Mongo.", entities.Count);
        return entities;
    }

    public async Task Update(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to update {type} '{@entity}'.", typeof(T).Name, entity);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        var result = await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions(), cancellationToken);

        if (result.ModifiedCount == 0)
            throw new KeyNotFoundException($"Could not find {typeof(T).Name} '{entity.Id}' to update.");

        _logger.LogDebug("Successfully updated {type} '{id}'.", typeof(T).Name, entity.Id);
    }

    public async Task Delete(Id<T> id, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to delete {type} '{id}'.", typeof(T).Name, id);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var result = await collection.DeleteOneAsync(filter, cancellationToken);

        if (result.DeletedCount == 0)
            throw new KeyNotFoundException($"Could not find {typeof(T).Name} '{id}' to delete.");

        _logger.LogDebug("Successfully deleted {type} '{id}'.", typeof(T).Name, id);
    }

    public IQueryable<T> AsQueryable(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to get queryable for type {type}.", typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var query = _mongoRepository.AsQueryable();

        _logger.LogDebug("Successfully retrieved queryable for type {type}.", typeof(T).Name);
        return query;
    }
}