using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations.Schemas;

public sealed record GetAllConversationsResponse(
    Id<Conversation> Id,
    string Name,
    bool IsPublic,
    Id<Folder>? FolderId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
