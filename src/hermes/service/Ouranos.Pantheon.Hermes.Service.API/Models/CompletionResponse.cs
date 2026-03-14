using Ouranos.Pantheon.Hermes.Service.Application.Commands.Conversations.GenerateCompletion;

namespace Ouranos.Pantheon.Hermes.Service.API.Models;

public sealed record CompletionResponse(
    [property: StreamResult] IAsyncEnumerable<GenerateCompletionResponse> Chunks
);