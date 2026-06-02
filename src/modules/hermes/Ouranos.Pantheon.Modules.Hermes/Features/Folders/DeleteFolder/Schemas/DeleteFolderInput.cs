using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder.Schemas;

public sealed record DeleteFolderInput(Id<Folder> FolderId);
