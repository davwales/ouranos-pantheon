using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Interfaces.Common;

public interface ICrudRepository<T> where T : BaseEntity<Id<T>>
{
    Task Create(T entity, CancellationToken cancellationToken = default);

    Task<T> Read(Id<T> id, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> ReadAll(CancellationToken cancellationToken = default);

    Task Update(T entity, CancellationToken cancellationToken = default);

    Task Delete(Id<T> id, CancellationToken cancellationToken = default);

    Task Upsert(T entity, CancellationToken cancellationToken = default);

    IQueryable<T> AsQueryable(CancellationToken cancellationToken = default);
}