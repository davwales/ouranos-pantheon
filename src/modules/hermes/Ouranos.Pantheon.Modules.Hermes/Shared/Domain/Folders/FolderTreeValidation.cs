using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;

public static class FolderTreeValidation
{
    public static async Task<bool> CheckForLoopAsync(
        Id<Folder> folderId,
        HermesDbContext dbContext,
        Id<Folder>? targetFolderId = null,
        CancellationToken cancellationToken = default
    )
    {
        targetFolderId ??= folderId;
        var initial = await dbContext.Folders.FirstOrDefaultAsync(
            f => f.Id == folderId,
            cancellationToken
        );
        Id<Folder>? currentId = initial?.ParentFolderId;

        while (currentId is not null)
        {
            if (currentId == targetFolderId)
            {
                return true;
            }

            var parent = await dbContext.Folders.FirstOrDefaultAsync(
                f => f.Id == currentId,
                cancellationToken
            );
            currentId = parent?.ParentFolderId;
        }

        return false;
    }

    public static async Task<int> CalculateDepthAsync(
        Id<Folder> folderId,
        HermesDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        var depth = 0;
        Id<Folder>? currentId = folderId;

        while (currentId is not null)
        {
            depth++;
            var parent = await dbContext.Folders.FirstOrDefaultAsync(
                f => f.Id == currentId,
                cancellationToken
            );
            currentId = parent?.ParentFolderId;
        }

        return depth;
    }

    /// <summary>
    /// Levels in the folder's subtree, counting the folder itself (a folder without subfolders
    /// has height 1). Stops counting past <see cref="Folder.MaxDepth"/>.
    /// </summary>
    public static async Task<int> CalculateSubtreeHeightAsync(
        Id<Folder> folderId,
        HermesDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        var height = 1;
        List<Id<Folder>?> level = [folderId];

        while (height <= Folder.MaxDepth)
        {
            var children = await dbContext
                .Folders.Where(f => level.Contains(f.ParentFolderId))
                .Select(f => (Id<Folder>?)f.Id)
                .ToListAsync(cancellationToken);

            if (children.Count == 0)
            {
                break;
            }

            height++;
            level = children;
        }

        return height;
    }
}
