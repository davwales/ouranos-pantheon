using MediatR;
using Microsoft.Extensions.Logging;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Application.Queries.Common.GetAllEntities;

public sealed class GetAllEntitiesHandler<T> : IRequestHandler<GetAllEntitiesInput<T>, IQueryable<T>>
    where T : BaseEntity<Id<T>>
{
    private readonly ILogger<T> _logger;
    private readonly ICrudRepository<T> _repository;

    public GetAllEntitiesHandler(
        ILogger<T> logger,
        ICrudRepository<T> repository
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(repository);

        _logger = logger;
        _repository = repository;
    }

    public async Task<IQueryable<T>> Handle(
        GetAllEntitiesInput<T> request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all entities request '{@request}' for type '{type}'.", request,
            typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var query = _repository.AsQueryable(cancellationToken);

        _logger.LogDebug("Successfully handled get all entities request.");
        return await Task.FromResult(query);
    }
}