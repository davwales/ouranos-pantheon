using Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

namespace Ouranos.Pantheon.Service.Hermes.API.Models;

public sealed record CompletionResponse(
    [property: StreamResult] IAsyncEnumerable<GenerateCompletionResponse> Chunks
);