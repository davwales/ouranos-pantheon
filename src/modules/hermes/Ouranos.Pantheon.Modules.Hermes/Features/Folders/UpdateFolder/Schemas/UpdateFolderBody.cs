using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder.Schemas;

public sealed record UpdateFolderBody(
    string Name,
    bool IsPublic,
    Id<Folder>? ParentFolderId = null
);
