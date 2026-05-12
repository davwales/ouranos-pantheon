namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;

public sealed record GetConversationTokenUsageResponse(
    int InputTokens,
    int OutputTokens,
    int TotalTokens
);
