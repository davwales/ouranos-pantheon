using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Infra.Postgres.Common;

public class UnitOfWork<TContext> : IUnitOfWork where TContext : OuranosDbContext
{
    private readonly TContext _context;
    private readonly Dictionary<Type, object> _repositories = [];
    private readonly IServiceProvider _serviceProvider;

    public UnitOfWork(TContext context, IServiceProvider serviceProvider)
    {
        Guard.Against.Null(context);
        Guard.Against.Null(serviceProvider);

        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var repository in _repositories)
        {
            if (repository.Value is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync();
            }
        }

        _repositories.Clear();
        await _context.DisposeAsync();
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity<Id<TEntity>>
    {
        var type = typeof(TEntity);
        if (_repositories.TryGetValue(type, out var existingRepository) &&
            existingRepository is IRepository<TEntity> typedRepository)
        {
            return typedRepository;
        }

        var repository = _serviceProvider.GetRequiredService<IRepository<TEntity>>();
        _repositories[type] = repository;
        return repository;
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}