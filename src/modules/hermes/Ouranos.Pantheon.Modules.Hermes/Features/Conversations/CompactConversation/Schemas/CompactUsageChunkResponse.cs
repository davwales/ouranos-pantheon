namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;

public sealed record CompactUsageChunkResponse(int InputTokens, int OutputTokens, int TotalTokens)
    : CompactConversationResponse;
