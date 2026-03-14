using HotChocolate.Types;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record CompletionResponse(
    [property: StreamResult] IAsyncEnumerable<GenerateCompletionResponse> Chunks
);
