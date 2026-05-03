namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record UsageChunkResponse(
    int InputTokens,
    int OutputTokens,
    int TotalTokens
) : GenerateCompletionResponse;