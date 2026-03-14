using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant.Schemas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant;

[ExtendObjectType<Mutation>]
public sealed class UpdateAssistantMutation
{
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
    public async Task<IdResponse<Shared.Domain.Assistants.Assistant>> UpdateAssistant(
        [Service] IScopedDispatcher dispatcher,
        UpdateAssistantInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }
}
