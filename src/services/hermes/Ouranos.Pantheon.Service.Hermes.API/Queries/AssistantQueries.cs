using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.API.Queries;

[ExtendObjectType<Query>]
public sealed class AssistantQueries
{
    /// <summary>
    ///     Gets a assistant.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="assistantId">The query to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The assistant matching the given query.</returns>
    public async Task<Assistant> GetAssistant(
        [Service] IScopedDispatcher dispatcher,
        Id<Assistant> assistantId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetEntityInput<Assistant>(assistantId), cancellationToken);
    }

    /// <summary>
    ///     Gets a queryable list of assistants.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of assistants.</returns>
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Assistant>> GetAllAssistants(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<Assistant>(), cancellationToken);
        return wrapper.Value;
    }
}