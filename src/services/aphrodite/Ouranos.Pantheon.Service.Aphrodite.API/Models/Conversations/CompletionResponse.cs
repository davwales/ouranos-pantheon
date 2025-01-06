using Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Service.Aphrodite.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Aphrodite.API.Models.Conversations;

public sealed record CompletionResponse(Role Role, IAsyncEnumerable<GenerateCompletionResponse> Content)
{
    [StreamResult] public IAsyncEnumerable<GenerateCompletionResponse> Content { get; init; } = Content;
}