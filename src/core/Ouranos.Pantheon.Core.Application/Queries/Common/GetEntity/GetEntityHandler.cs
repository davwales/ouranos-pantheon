using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;

public sealed class GetEntityHandler<T> : IRequestHandler<GetEntityInput<T>, T> where T : BaseEntity<Id<T>>
{
    private readonly ILogger<GetEntityHandler<T>> _logger;
    private readonly ICrudRepository<T> _repository;

    public GetEntityHandler(
        ILogger<GetEntityHandler<T>> logger,
        ICrudRepository<T> repository
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(repository);

        _logger = logger;
        _repository = repository;
    }

    public async Task<T> Handle(
        GetEntityInput<T> request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get entity request '{@request}' for type '{type}'.", request,
            typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        var entity = await _repository.Read(request.EntityId, cancellationToken);

        _logger.LogDebug("Successfully handled get entity request.");
        return entity;
    }
}