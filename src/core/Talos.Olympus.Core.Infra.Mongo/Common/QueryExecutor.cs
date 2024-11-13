using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Talos.Olympus.Core.Application.Interfaces.Common;

namespace Talos.Olympus.Core.Infra.Mongo.Common;

public sealed class QueryExecutor : IQueryExecutor
{
    public Task<int> CountAsync<T>(
        IQueryable<T> queryable,
        CancellationToken cancellationToken = default
    )
    {
        if (queryable is not IMongoQueryable<T> mongoQueryable)
            throw new InvalidOperationException("Cannot perform CountAsync on a non-Mongo queryable.");

        return mongoQueryable.CountAsync(cancellationToken);
    }

    public Task<T> FirstOrDefaultAsync<T>(
        IQueryable<T> queryable,
        CancellationToken cancellationToken = default
    )
    {
        if (queryable is not IMongoQueryable<T> mongoQueryable)
            throw new InvalidOperationException("Cannot perform FirstOrDefaultAsync on a non-Mongo queryable.");

        return mongoQueryable.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<T>> ToListAsync<T>(
        IQueryable<T> queryable,
        CancellationToken cancellationToken = default
    )
    {
        if (queryable is not IMongoQueryable<T> mongoQueryable)
            throw new InvalidOperationException("Cannot perform ToListAsync on a non-Mongo queryable.");

        return mongoQueryable.ToListAsync(cancellationToken);
    }
}