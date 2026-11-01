using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;

/// <summary>
/// Public mode shows a folder only when it and every folder above it are public, so a private
/// folder hides its whole subtree, whatever the subfolders' own flags say.
/// </summary>
public static class FolderVisibility
{
    public static async Task<List<Id<Folder>>> GetHiddenFolderIdsAsync(
        this HermesDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        var folders = await dbContext
            .Folders.AsNoTracking()
            .Select(f => new
            {
                f.Id,
                f.ParentFolderId,
                f.IsPublic,
            })
            .ToDictionaryAsync(f => f.Id, cancellationToken);

        var isHiddenById = new Dictionary<Id<Folder>, bool>();

        foreach (var folderId in folders.Keys)
        {
            var chain = new List<Id<Folder>>();
            var isHidden = false;
            Id<Folder>? currentId = folderId;

            while (currentId is { } id && folders.TryGetValue(id, out var current))
            {
                if (isHiddenById.TryGetValue(id, out var known))
                {
                    isHidden = known;
                    break;
                }

                if (chain.Contains(id))
                {
                    break;
                }

                chain.Add(id);

                if (!current.IsPublic)
                {
                    isHidden = true;
                    break;
                }

                currentId = current.ParentFolderId;
            }

            foreach (var id in chain)
            {
                isHiddenById[id] = isHidden;
            }
        }

        return [.. isHiddenById.Where(kv => kv.Value).Select(kv => kv.Key)];
    }

    public static async Task<bool> IsFolderHiddenAsync(
        this HermesDbContext dbContext,
        Id<Folder>? folderId,
        CancellationToken cancellationToken = default
    )
    {
        var visited = new HashSet<Id<Folder>>();
        var currentId = folderId;

        while (currentId is { } id && visited.Add(id))
        {
            var folder = await dbContext
                .Folders.AsNoTracking()
                .Where(f => f.Id == id)
                .Select(f => new { f.ParentFolderId, f.IsPublic })
                .FirstOrDefaultAsync(cancellationToken);

            if (folder is null)
            {
                return false;
            }

            if (!folder.IsPublic)
            {
                return true;
            }

            currentId = folder.ParentFolderId;
        }

        return false;
    }
}
