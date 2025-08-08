using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Hermes.Application.Commands.Assistants.CreateAssistant;
using Ouranos.Pantheon.Service.Hermes.Application.Commands.Assistants.UpdateAssistant;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.API.Mutations;

[ExtendObjectType<Mutation>]
public sealed class AssistantMutations
{
    /// <summary>
    ///     Creates a assistant.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the newly created assistant.</returns>
    public async Task<IdResponse<Assistant>> CreateAssistant(
        [Service] IScopedDispatcher dispatcher,
        CreateAssistantInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }

    /// <summary>
    ///     Updates a assistant.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the updated assistant.</returns>
    public async Task<IdResponse<Assistant>> UpdateAssistant(
        [Service] IScopedDispatcher dispatcher,
        UpdateAssistantInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }

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
        return await dispatcher.Send(new DeleteEntityInput<Assistant>(assistantId), cancellationToken);
    }
}