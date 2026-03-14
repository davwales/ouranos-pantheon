using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

[ExtendObjectType<Mutation>]
public sealed class GenerateCompletionMutation
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
        [Service] IScopedDispatcher dispatcher,
        GenerateCompletionInput input,
        CancellationToken cancellationToken = default
    )
    {
        var stream = dispatcher.CreateStream(input, cancellationToken);
        return new CompletionResponse(stream);
    }
}