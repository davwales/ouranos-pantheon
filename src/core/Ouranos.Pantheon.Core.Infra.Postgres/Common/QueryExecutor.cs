using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;

namespace Ouranos.Pantheon.Core.Infra.Postgres.Common;

public sealed class QueryExecutor : IQueryExecutor
{
    public async Task<T?> FirstOrDefault<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default
    )
    {
        if (query is IAsyncEnumerable<T>)
        {
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        return query.FirstOrDefault();
    }

    public async Task<List<T>> ToList<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default
    )
    {
        if (query is IAsyncEnumerable<T>)
        {
            return await query.ToListAsync(cancellationToken);
        }

        return [.. query];
    }
}