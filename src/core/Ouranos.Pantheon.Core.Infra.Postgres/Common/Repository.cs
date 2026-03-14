using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Infra.Postgres.Common;

public sealed class Repository<TContext, TEntity> : IRepository<TEntity>, IAsyncDisposable
    where TContext : OuranosDbContext
    where TEntity : BaseEntity<Id<TEntity>>
{
    public readonly TContext Context;
    public readonly DbSet<TEntity> DbSet;

    private readonly ILogger<Repository<TContext, TEntity>> _logger;

    public Repository(
        ILogger<Repository<TContext, TEntity>> logger,
        TContext context
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(context);

        _logger = logger;
        Context = context;
        DbSet = Context.Set<TEntity>();
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    public Id<TEntity> CreateId()
    {
        return new Id<TEntity>(Guid.NewGuid().ToString());
    }

    public async Task Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Creating entity of type '{EntityType}' with input: {Entity}", typeof(TEntity).Name, entity);
        await DbSet.AddAsync(entity, cancellationToken);
        _logger.LogDebug(
            "Entity of type '{EntityType}' with ID '{EntityId}' created successfully",
            typeof(TEntity).Name,
            entity.Id
        );
    }

    public async Task CreateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        _logger.LogTrace(
            "Creating '{EntityCount}' entities of type '{EntityType}'",
            entityList.Count,
            typeof(TEntity).Name
        );
        await DbSet.AddRangeAsync(entityList, cancellationToken);
        _logger.LogDebug(
            "'{EntityCount}' entities of type '{EntityType}' created successfully",
            entityList.Count,
            typeof(TEntity).Name
        );
    }

    public async Task<TEntity> Read(Id<TEntity> id, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Reading entity of type '{EntityType}' with ID '{EntityId}'", typeof(TEntity).Name, id);
        var entity = await DbSet.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);

        if (entity is null)
        {
            _logger.LogDebug("Entity with ID '{EntityId}' was not found.", id);
            throw new KeyNotFoundException($"Entity with ID {id} was not found.");
        }

        _logger.LogDebug(
            "Successfully read entity of type '{EntityType}' with ID '{EntityId}'",
            typeof(TEntity).Name,
            id
        );
        return entity;
    }

    public async Task<TEntity?> FirstOrDefault(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Getting first or default entity of type '{EntityType}' with predicate '{Predicate}'",
            typeof(TEntity).Name,
            predicate.ToString()
        );
        var entity = await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        _logger.LogDebug("Successfully retrieved first or default entity of type '{EntityType}'", typeof(TEntity).Name);
        return entity;
    }

    public async Task<bool> Any(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Checking if any entity of type '{EntityType}' exists with predicate '{Predicate}'",
            typeof(TEntity).Name,
            predicate.ToString()
        );
        var exists = await DbSet.AnyAsync(predicate, cancellationToken);
        _logger.LogDebug("Successfully checked if any entity of type '{EntityType}' exists", typeof(TEntity).Name);
        return exists;
    }

    public async Task<IEnumerable<TEntity>> ReadAll(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Reading all entities of type '{EntityType}'", typeof(TEntity).Name);
        var entities = await DbSet.ToListAsync(cancellationToken);
        _logger.LogDebug(
            "Read '{EntityCount}' entities of type '{EntityType}' successfully",
            entities.Count,
            typeof(TEntity).Name
        );
        return entities;
    }

    public async Task<List<TEntity>> ReadAll(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Reading all entities of type '{EntityType}' matching predicate '{Predicate}'",
            typeof(TEntity).Name,
            predicate.ToString()
        );
        var entities = await DbSet.Where(predicate).ToListAsync(cancellationToken);
        _logger.LogDebug(
            "Read '{EntityCount}' entities of type '{EntityType}' matching predicate successfully",
            entities.Count,
            typeof(TEntity).Name
        );
        return entities;
    }

    public Task Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Updating entity of type '{EntityType}' with input: {Entity}", typeof(TEntity).Name, entity);
        DbSet.Update(entity);
        _logger.LogDebug(
            "Entity of type '{EntityType}' with ID '{EntityId}' updated successfully",
            typeof(TEntity).Name,
            entity.Id
        );
        return Task.CompletedTask;
    }

    public async Task Delete(Id<TEntity> id, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Deleting entity of type '{EntityType}' with ID '{EntityId}'", typeof(TEntity).Name, id);
        var entity = await DbSet.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        if (entity is not null)
        {
            DbSet.Remove(entity);
            _logger.LogDebug(
                "Entity of type '{EntityType}' with ID '{EntityId}' deleted successfully",
                typeof(TEntity).Name,
                id
            );
        }
        else
        {
            _logger.LogWarning(
                "Entity of type '{EntityType}' with ID '{EntityId}' not found for deletion",
                typeof(TEntity).Name,
                id
            );
        }
    }

    public Task<long> Delete(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Deleting entities of type '{EntityType}' matching predicate '{Predicate}'",
            typeof(TEntity).Name,
            predicate.ToString()
        );

        DbSet.RemoveRange(DbSet.Where(predicate));
        _logger.LogDebug(
            "Entities of type '{EntityType}' deleted successfully",
            typeof(TEntity).Name
        );

        return Task.FromResult(0L);
    }

    public async Task Upsert(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Upserting entity of type '{EntityType}' with input: {Entity}", typeof(TEntity).Name, entity);
        var exists = await DbSet
            .AsNoTracking()
            .AnyAsync(e => e.Id.Equals(entity.Id), cancellationToken);

        if (exists)
        {
            _logger.LogTrace(
                "Updating entity of type '{EntityType}' with ID '{EntityId}'",
                typeof(TEntity).Name,
                entity.Id
            );
            DbSet.Update(entity);
        }
        else
        {
            _logger.LogTrace(
                "Creating entity of type '{EntityType}' with ID '{EntityId}'",
                typeof(TEntity).Name,
                entity.Id
            );
            await DbSet.AddAsync(entity, cancellationToken);
        }

        _logger.LogDebug(
            "Upserted entity of type '{EntityType}' with ID '{EntityId}' successfully",
            typeof(TEntity).Name,
            entity.Id
        );
    }

    public IQueryable<TEntity> AsQueryable(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Returning IQueryable for entity type '{EntityType}'", typeof(TEntity).Name);
        var queryable = DbSet.AsQueryable();
        _logger.LogDebug("Returned IQueryable for entity type '{EntityType}' successfully", typeof(TEntity).Name);
        return queryable;
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Saving changes to the database");
        var changedCount = await Context.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Saved '{ChangedCount}' changes to the database successfully", changedCount);
    }
}