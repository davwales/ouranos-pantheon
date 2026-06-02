using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder.Schemas;

public sealed record GetFolderInput(Id<Folder> FolderId);
