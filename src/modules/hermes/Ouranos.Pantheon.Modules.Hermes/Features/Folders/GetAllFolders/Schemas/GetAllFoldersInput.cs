using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders.Schemas;

public sealed record GetAllFoldersInput(
    Id<Folder>? ParentFolderId = null,
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
