using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;

public sealed class GetEntityHandler<T> : QueryHandler<GetEntityInput<T>, T> where T : BaseEntity<Id<T>>
{
    private readonly ILogger<GetEntityHandler<T>> _logger;
    private readonly IRepository<T> _repository;

    public GetEntityHandler(
        ILogger<GetEntityHandler<T>> logger,
        IRepository<T> repository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(repository);

        _logger = logger;
        _repository = repository;
    }

    public override async Task<T> Handle(
        GetEntityInput<T> query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get entity query '{@query}' for type '{type}'.", query,
            typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var entity = await _repository.Read(query.EntityId, cancellationToken);

        _logger.LogDebug("Successfully handled get entity query.");
        return entity;
    }
}