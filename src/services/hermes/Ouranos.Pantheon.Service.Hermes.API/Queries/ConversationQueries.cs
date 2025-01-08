using MediatR;
using Ouranos.Pantheon.Core.API.Models;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Service.Hermes.Application.Queries.Conversations.GetCompletion;

namespace Ouranos.Pantheon.Service.Hermes.API.Queries;

[ExtendObjectType<Query>]
public sealed class ConversationQueries
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
    /// <returns>Generated completion response.</returns>
    public StreamResponse<Chunk<string>> GetCompletion(
        [Service] IMediator mediator,
        GetCompletionInput input,
        CancellationToken cancellationToken = default
    )
    {
        return new StreamResponse<Chunk<string>>(mediator.CreateStream(input, cancellationToken));
    }
}