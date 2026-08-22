using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder.Schemas;

public sealed record UpdateFolderInput(
    Id<Folder> FolderId,
    string Name,
    bool IsPublic,
    Id<Folder>? ParentFolderId = null
);
