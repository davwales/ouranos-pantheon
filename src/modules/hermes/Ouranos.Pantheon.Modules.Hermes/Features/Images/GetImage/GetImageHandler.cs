using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage;

public sealed class GetImageHandler(
    ILogger<GetImageHandler> logger,
    HermesDbContext dbContext,
    IFlagsmithClient flagsmith
) : IPantheonHandler<GetImageInput, GetImageResponse>
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly IFlagsmithClient _flagsmith = Guard.Against.Null(flagsmith);
    private readonly ILogger<GetImageHandler> _logger = Guard.Against.Null(logger);

    public async Task<GetImageResponse> Handle(
        GetImageInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get image query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        if (
            await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode)
            && await IsInPrivateConversationAsync(query, cancellationToken)
        )
        {
            throw new NotFoundException(query.ImageId.ToString(), nameof(GeneratedImage));
        }

        var image = await _dbContext
            .GeneratedImages.AsNoTracking()
            .Where(i => i.Id == query.ImageId)
            .Select(i => new GetImageResponse(i.Content, i.MediaType))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(query.ImageId, image);

        _logger.LogDebug("Successfully handled get image request.");
        return image;
    }

    private async Task<bool> IsInPrivateConversationAsync(
        GetImageInput query,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .Conversations.AsNoTracking()
            .Where(c =>
                c.Messages.Any(m =>
                    m.Parts.OfType<ImageMessagePart>().Any(p => p.ImageId == query.ImageId)
                )
            )
            .AnyAsync(
                c => !c.IsPublic || _dbContext.Folders.Any(f => f.Id == c.FolderId && !f.IsPublic),
                cancellationToken
            );
    }
}
