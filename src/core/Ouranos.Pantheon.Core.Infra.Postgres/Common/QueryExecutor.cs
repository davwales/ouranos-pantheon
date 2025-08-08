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
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> ToList<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default
    )
    {
        return await query.ToListAsync(cancellationToken);
    }
}