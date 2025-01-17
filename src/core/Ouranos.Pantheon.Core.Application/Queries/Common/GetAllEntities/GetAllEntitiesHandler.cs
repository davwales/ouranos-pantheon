using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;

public sealed class GetAllEntitiesHandler<T> : IQueryHandler<GetAllEntitiesInput<T>, WrapperResponse<IQueryable<T>>>
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

    public async Task Consume(ConsumeContext<GetAllEntitiesInput<T>> context)
    {
        _logger.LogTrace("Attempting to handle get all entities query '{@query}' for type '{type}'.", context.Message,
            typeof(T).Name);
        context.CancellationToken.ThrowIfCancellationRequested();

        var result = _repository.AsQueryable(context.CancellationToken);

        _logger.LogDebug("Successfully handled get all entities query.");
        await context.RespondAsync(new WrapperResponse<IQueryable<T>>(result));
    }
}