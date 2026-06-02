using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders.Schemas;

public sealed record FolderSummary(
    Id<Folder> Id,
    string Name,
    bool IsPublic,
    Id<Folder>? ParentFolderId,
    int ConversationCount,
    int SubfolderCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
