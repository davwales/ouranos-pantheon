using Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.API.Models.Conversations;

public sealed record CompletionResponse(Role Role, IAsyncEnumerable<GenerateCompletionResponse> Content)
{
    [StreamResult] public IAsyncEnumerable<GenerateCompletionResponse> Content { get; init; } = Content;
}