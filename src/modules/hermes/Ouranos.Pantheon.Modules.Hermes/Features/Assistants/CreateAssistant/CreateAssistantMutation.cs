using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant.Schemas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant;

[ExtendObjectType<Mutation>]
public sealed class CreateAssistantMutation
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
    public async Task<IdResponse<Shared.Domain.Assistants.Assistant>> CreateAssistant(
        [Service] IScopedDispatcher dispatcher,
        CreateAssistantInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }
}