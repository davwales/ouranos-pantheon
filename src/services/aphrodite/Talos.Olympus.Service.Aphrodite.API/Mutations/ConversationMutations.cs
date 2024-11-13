using MediatR;
using Talos.Olympus.Core.API.Mutations;
using Talos.Olympus.Service.Aphrodite.API.Models.Conversations;
using Talos.Olympus.Service.Aphrodite.Application.Commands.Conversations.GenerateCompletion;
using Talos.Olympus.Service.Aphrodite.Domain.Conversations;

namespace Talos.Olympus.Service.Aphrodite.API.Mutations;

[ExtendObjectType<Mutation>]
public sealed class ConversationMutations
{
    /// <summary>
    ///     Generates a completion given some conversation input.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="input">Input data used to generate the completion.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Generated completion stream and accompanying role.</returns>
    public CompletionResponse GenerateCompletion(
        [Service] IMediator mediator,
        GenerateCompletionInput input,
        CancellationToken cancellationToken = default
    )
    {
        return new CompletionResponse(Role.Assistant, mediator.CreateStream(input, cancellationToken));
    }
}