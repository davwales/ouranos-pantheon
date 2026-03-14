using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
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
