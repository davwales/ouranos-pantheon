using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder.Schemas;

public sealed record CreateFolderInput(
    string Name,
    bool IsPublic = true,
    Id<Folder>? ParentFolderId = null
);
