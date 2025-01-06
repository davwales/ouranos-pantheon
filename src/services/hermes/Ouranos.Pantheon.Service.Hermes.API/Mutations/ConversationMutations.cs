using MediatR;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Service.Hermes.API.Models.Conversations;
using Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.API.Mutations;

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
    ) => new CompletionResponse(Role.Assistant, mediator.CreateStream(input, cancellationToken));
}