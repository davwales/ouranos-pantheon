namespace Ouranos.Pantheon.Core.Application.Interfaces.Common;

public interface IQueryExecutor
{
    Task<T?> FirstOrDefault<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default
    );

    Task<List<T>> ToList<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default
    );
}