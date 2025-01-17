using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.API.Models;
using Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

namespace Ouranos.Pantheon.Service.Hermes.API.Mutations;

[ExtendObjectType<Mutation>]
public sealed class ConversationMutations
{
    /// <summary>
    ///     Generates a completion given some conversation input.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Input data used to generate the completion.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Generated completion response.</returns>
    public CompletionResponse GenerateCompletion(
        [Service] IDispatcher dispatcher,
        GenerateCompletionInput input,
        CancellationToken cancellationToken = default
    )
    {
        var stream = dispatcher.CreateStream(input, cancellationToken);
        return new CompletionResponse(stream);
    }
}