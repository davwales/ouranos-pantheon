using System.Linq.Expressions;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Interfaces.Common;

public interface IRepository<T> where T : BaseEntity<Id<T>>
{
    Id<T> CreateId();

    Task Create(
        T entity,
        CancellationToken cancellationToken = default
    );

    Task CreateMany(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    );

    Task<T> Read(
        Id<T> id,
        CancellationToken cancellationToken = default
    );

    Task<T?> FirstOrDefault(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<bool> Any(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<T>> ReadAll(
        CancellationToken cancellationToken = default
    );

    Task<List<T>> ReadAll(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task Update(
        T entity,
        CancellationToken cancellationToken = default
    );

    Task Delete(
        Id<T> id,
        CancellationToken cancellationToken = default
    );

    Task<long> Delete(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task Upsert(
        T entity,
        CancellationToken cancellationToken = default
    );

    IQueryable<T> AsQueryable(
        CancellationToken cancellationToken = default
    );

    Task SaveChanges(
        CancellationToken cancellationToken = default
    );
}