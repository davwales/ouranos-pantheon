using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations.Schemas;

public sealed record GetAllConversationsInput(
    Id<Folder>? FolderId = null,
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
