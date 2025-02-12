using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Common;

public sealed class Repository<T> : IRepository<T> where T : BaseEntity<Id<T>>
{
    private readonly ILogger<Repository<T>> _logger;
    private readonly IMongoRepository<T> _mongoRepository;

    public Repository(ILogger<Repository<T>> logger, IMongoRepository<T> mongoRepository)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mongoRepository);

        _logger = logger;
        _mongoRepository = mongoRepository;
    }

    public Id<T> CreateId()
    {
        var mongoId = ObjectId.GenerateNewId().ToString();
        return new Id<T>(mongoId);
    }

    public async Task Create(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to create {type} '{@entity}'.", typeof(T).Name, entity);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        await collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

        _logger.LogDebug("Successfully created {type} '{id}'.", typeof(T).Name, entity.Id);
    }

    public async Task CreateMany(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to create many {type}.", typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        await collection.InsertManyAsync(entities, cancellationToken: cancellationToken);

        _logger.LogDebug("Successfully created many {type}.", typeof(T).Name);
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

        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Could not find {typeof(T).Name} '{entity.Id}' to update.");
        }

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
        {
            throw new KeyNotFoundException($"Could not find {typeof(T).Name} '{id}' to delete.");
        }

        _logger.LogDebug("Successfully deleted {type} '{id}'.", typeof(T).Name, id);
    }

    public async Task Upsert(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to upsert {type} '{@entity}'.", typeof(T).Name, entity);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions
        {
            IsUpsert = true
        }, cancellationToken);

        _logger.LogDebug("Successfully performed upsert for {type} '{id}'.", typeof(T).Name, entity.Id);
    }

    public IQueryable<T> AsQueryable(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to get queryable for type {type}.", typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var query = _mongoRepository.AsQueryable();

        _logger.LogDebug("Successfully retrieved queryable for type {type}.", typeof(T).Name);
        return query;
    }

    public async Task<T?> FirstOrDefault(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to find {type} or default using a predicate in Mongo.", typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        var filter = Builders<T>.Filter.Where(predicate);
        var entity = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        _logger.LogDebug("Successfully find {type} or default using a predicate in Mongo.", typeof(T).Name);
        return entity;
    }

    public async Task<bool> Any(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to see if any {type} match the given predicate in Mongo.", typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoRepository.GetCollection();
        var filter = Builders<T>.Filter.Where(predicate);
        var wasFound = await collection.Find(filter).AnyAsync(cancellationToken);

        _logger.LogDebug("Successfully determined if any {type} matched the given predicate in Mongo.", typeof(T).Name);
        return wasFound;
    }
}