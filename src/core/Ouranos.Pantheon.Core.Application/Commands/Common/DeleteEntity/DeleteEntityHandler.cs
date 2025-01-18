using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;

public sealed class DeleteEntityHandler<T> : CommandHandler<DeleteEntityInput<T>, IdResponse<T>>
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

    protected override async Task<IdResponse<T>> Handle(
        DeleteEntityInput<T> command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete entity command '{@command}' for type '{type}'.", command,
            typeof(T).Name);
        cancellationToken.ThrowIfCancellationRequested();

        await _repository.Delete(command.EntityId, cancellationToken);

        _logger.LogDebug("Successfully handled delete entity command.");
        return new IdResponse<T>(command.EntityId);
    }
}