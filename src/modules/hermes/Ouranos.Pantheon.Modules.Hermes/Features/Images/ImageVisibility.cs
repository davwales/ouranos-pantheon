using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images;

public static class ImageVisibility
{
    /// <summary>
    /// Whether a private conversation, or one in a private folder, shows the image. Public mode
    /// hides such images.
    /// </summary>
    public static Task<bool> IsImageInPrivateConversationAsync(
        this HermesDbContext dbContext,
        Id<GeneratedImage> imageId,
        CancellationToken cancellationToken
    )
    {
        return dbContext
            .Conversations.AsNoTracking()
            .Where(c =>
                c.Messages.Any(m =>
                    m.Parts.OfType<ImageMessagePart>().Any(p => p.ImageId == imageId)
                )
            )
            .AnyAsync(
                c => !c.IsPublic || dbContext.Folders.Any(f => f.Id == c.FolderId && !f.IsPublic),
                cancellationToken
            );
    }
}
