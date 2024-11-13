using Talos.Olympus.Service.Aphrodite.Application.Commands.Conversations.GenerateCompletion;
using Talos.Olympus.Service.Aphrodite.Domain.Conversations;

namespace Talos.Olympus.Service.Aphrodite.API.Models.Conversations;

public sealed record CompletionResponse(Role Role, IAsyncEnumerable<GenerateCompletionResponse> Content)
{
    [StreamResult] public IAsyncEnumerable<GenerateCompletionResponse> Content { get; init; } = Content;
}