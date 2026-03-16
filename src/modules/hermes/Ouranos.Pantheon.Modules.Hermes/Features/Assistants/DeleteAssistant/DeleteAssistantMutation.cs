using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Mutations;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant;

[ExtendObjectType<Mutation>]
public sealed class DeleteAssistantMutation
{
    /// <summary>
    ///     Deletes a assistant.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="assistantId">Id of the assistant to delete.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the recently deleted assistant.</returns>
    public async Task<IdResponse<Assistant>> DeleteAssistant(
        [Service] IScopedDispatcher dispatcher,
        Id<Assistant> assistantId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new DeleteAssistantInput(assistantId), cancellationToken);
    }
}
