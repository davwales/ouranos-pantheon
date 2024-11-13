namespace Talos.Olympus.Core.Application.Interfaces.Common;

public interface IQueryExecutor
{
    Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
}