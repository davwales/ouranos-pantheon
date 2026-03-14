using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant;

[ExtendObjectType<Query>]
public sealed class GetAssistantQuery
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
        return await dispatcher.Send(new GetAssistantInput(assistantId), cancellationToken);
    }
}
