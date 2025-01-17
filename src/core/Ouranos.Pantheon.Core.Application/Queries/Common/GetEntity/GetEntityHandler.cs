using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;

public sealed class GetEntityHandler<T> : IQueryHandler<GetEntityInput<T>, T> where T : BaseEntity<Id<T>>
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

    public async Task Consume(ConsumeContext<GetEntityInput<T>> context)
    {
        _logger.LogTrace("Attempting to handle get entity query '{@query}' for type '{type}'.", context.Message,
            typeof(T).Name);
        context.CancellationToken.ThrowIfCancellationRequested();

        var entity = await _repository.Read(context.Message.EntityId, context.CancellationToken);

        _logger.LogDebug("Successfully handled get entity query.");
        await context.RespondAsync(entity);
    }
}