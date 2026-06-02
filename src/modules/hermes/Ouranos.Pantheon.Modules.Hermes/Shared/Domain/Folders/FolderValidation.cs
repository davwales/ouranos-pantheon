using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;

public static class FolderValidation
{
    public static async Task<Folder?> ValidateFolderExistsAsync(
        HermesDbContext dbContext,
        Id<Folder>? folderId,
        CancellationToken cancellationToken = default
    )
    {
        if (folderId is null)
        {
            return null;
        }

        var folder = await dbContext.Folders.FirstOrDefaultAsync(
            f => f.Id == folderId,
            cancellationToken
        );

        Guard.Against.NotFound(folderId.Value, folder);

        return folder;
    }
}
