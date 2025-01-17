using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;

public sealed class DeleteEntityHandler<T> : ICommandHandler<DeleteEntityInput<T>, IdResponse<T>>
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

    public async Task Consume(ConsumeContext<DeleteEntityInput<T>> context)
    {
        _logger.LogTrace("Attempting to handle delete entity command '{@command}' for type '{type}'.", context.Message,
            typeof(T).Name);
        context.CancellationToken.ThrowIfCancellationRequested();

        await _repository.Delete(context.Message.EntityId, context.CancellationToken);

        _logger.LogDebug("Successfully handled delete entity command.");
        await context.RespondAsync(new IdResponse<T>(context.Message.EntityId));
    }
}