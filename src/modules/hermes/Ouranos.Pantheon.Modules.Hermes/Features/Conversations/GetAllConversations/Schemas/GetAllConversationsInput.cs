namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations.Schemas;

public sealed record GetAllConversationsInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
