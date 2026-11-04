using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;

public interface IFolderTreeQueryService
{
    /// <summary>
    ///     Levels in the folder's subtree, counting the folder itself (a folder without subfolders
    ///     has height 1). Stops counting past <see cref="Folder.MaxDepth" />.
    /// </summary>
    Task<int> CalculateSubtreeHeightAsync(
        Id<Folder> folderId,
        CancellationToken cancellationToken = default
    );
}
