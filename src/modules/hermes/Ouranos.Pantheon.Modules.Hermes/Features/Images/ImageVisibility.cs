using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images;

public static class ImageVisibility
{
    /// <summary>
    /// Whether a private conversation, or one under a private folder, shows the image. Public
    /// mode hides such images.
    /// </summary>
    public static async Task<bool> IsImageInPrivateConversationAsync(
        this HermesDbContext dbContext,
        Id<GeneratedImage> imageId,
        CancellationToken cancellationToken
    )
    {
        var hiddenFolderIds = await dbContext.GetHiddenFolderIdsAsync(cancellationToken);

        return await dbContext
            .Conversations.AsNoTracking()
            .Where(c =>
                c.Messages.Any(m =>
                    m.Parts.OfType<ImageMessagePart>().Any(p => p.ImageId == imageId)
                )
            )
            .AnyAsync(
                c =>
                    !c.IsPublic
                    || (c.FolderId != null && hiddenFolderIds.Contains(c.FolderId.Value)),
                cancellationToken
            );
    }
}
