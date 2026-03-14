using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;

public sealed class DeleteEntityHandler<T> : CommandHandler<DeleteEntityInput<T>, IdResponse<T>>
    where T : BaseEntity<Id<T>>
{
    private readonly ILogger<DeleteEntityHandler<T>> _logger;
    private readonly IRepository<T> _repository;

    public DeleteEntityHandler(
        ILogger<DeleteEntityHandler<T>> logger,
        IRepository<T> repository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(repository);

        _logger = logger;
        _repository = repository;
    }

    public override async Task<IdResponse<T>> Handle(
        DeleteEntityInput<T> command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to handle delete entity command '{@command}' for type '{type}'.",
            command,
            typeof(T).Name
        );
        cancellationToken.ThrowIfCancellationRequested();

        await _repository.Delete(command.EntityId, cancellationToken);
        await _repository.SaveChanges(cancellationToken);

        _logger.LogDebug("Successfully handled delete entity command.");
        return new IdResponse<T>(command.EntityId);
    }
}