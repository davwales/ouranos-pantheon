using MediatR;
using Microsoft.Extensions.Logging;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Application.Commands.Common.DeleteEntity;

public sealed class DeleteEntityHandler<T> : IRequestHandler<DeleteEntityInput<T>, IdResponse<T>>
    where T : BaseEntity<Id<T>>
{
    private readonly ILogger<T> _logger;
    private readonly ICrudRepository<T> _repository;

    public DeleteEntityHandler(
        ILogger<T> logger,
        ICrudRepository<T> repository
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(repository);

        _logger = logger;
        _repository = repository;
    }

    public async Task<IdResponse<T>> Handle(
        DeleteEntityInput<T> request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete entity request '{@request}' for type '{type}'.", request,
            typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        await _repository.Delete(request.EntityId, cancellationToken);

        _logger.LogDebug("Successfully handled delete entity request.");
        return new IdResponse<T>(request.EntityId);
    }
}