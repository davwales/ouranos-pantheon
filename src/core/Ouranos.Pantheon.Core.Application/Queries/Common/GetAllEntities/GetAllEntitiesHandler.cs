using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;

public sealed class GetAllEntitiesHandler<T> : QueryHandler<GetAllEntitiesInput<T>, WrapperResponse<IQueryable<T>>>
    where T : BaseEntity<Id<T>>
{
    private readonly ILogger<GetAllEntitiesHandler<T>> _logger;
    private readonly IRepository<T> _repository;

    public GetAllEntitiesHandler(
        ILogger<GetAllEntitiesHandler<T>> logger,
        IRepository<T> repository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(repository);

        _logger = logger;
        _repository = repository;
    }

    public override Task<WrapperResponse<IQueryable<T>>> Handle(
        GetAllEntitiesInput<T> query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all entities query '{@query}' for type '{type}'.", query,
            typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var entityQuery = _repository.AsQueryable(cancellationToken);
        var response = new WrapperResponse<IQueryable<T>>(entityQuery);

        _logger.LogDebug("Successfully handled get all entities query.");
        return Task.FromResult(response);
    }
}