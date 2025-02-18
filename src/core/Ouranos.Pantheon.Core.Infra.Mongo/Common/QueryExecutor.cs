using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Common;

public sealed class QueryExecutor : IQueryExecutor
{
    private readonly ILogger<QueryExecutor> _logger;

    public QueryExecutor(ILogger<QueryExecutor> logger)
    {
        Guard.Against.Null(logger);
        _logger = logger;
    }

    public async Task<T?> FirstOrDefault<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to find '{resultType}' first or default of query on '{type}'.",
            typeof(T).Name, typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        if (query is not IMongoQueryable<T> mongoQuery)
        {
            throw new InvalidOperationException("Cannot perform FirstOrDefaultAsync on a non-Mongo queryable.");
        }

        var result = await mongoQuery.FirstOrDefaultAsync(cancellationToken);

        _logger.LogDebug("Successfully executed query to find first or default '{resultType}' from '{type}'.",
            typeof(T).Name, typeof(T).Name);
        return result;
    }

    public async Task<List<T>> ToList<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to find '{resultType}' list of query on '{type}'.",
            typeof(T).Name, typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        if (query is not IMongoQueryable<T> mongoQuery)
        {
            throw new InvalidOperationException("Cannot perform ToListAsync on a non-Mongo queryable.");
        }

        var result = await mongoQuery.ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully executed query list '{resultType}' from '{type}'.",
            typeof(T).Name, typeof(T).Name);
        return result;
    }
}