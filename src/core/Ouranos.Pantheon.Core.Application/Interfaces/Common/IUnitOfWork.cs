using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Interfaces.Common;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity<Id<TEntity>>;

    Task SaveChanges(CancellationToken cancellationToken = default);
}