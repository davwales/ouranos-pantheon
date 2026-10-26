using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails;

public sealed class GetImageDetailsHandler(
    ILogger<GetImageDetailsHandler> logger,
    HermesDbContext dbContext,
    IFlagsmithClient flagsmith
) : IPantheonHandler<GetImageDetailsInput, GetImageDetailsResponse>
{
    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly IFlagsmithClient _flagsmith = Guard.Against.Null(flagsmith);
    private readonly ILogger<GetImageDetailsHandler> _logger = Guard.Against.Null(logger);

    public async Task<GetImageDetailsResponse> Handle(
        GetImageDetailsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get image details query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        if (
            await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode)
            && await _dbContext.IsImageInPrivateConversationAsync(query.ImageId, cancellationToken)
        )
        {
            throw new NotFoundException(query.ImageId.ToString(), nameof(GeneratedImage));
        }

        var details = await _dbContext
            .GeneratedImages.AsNoTracking()
            .Where(i => i.Id == query.ImageId)
            .Select(i => new GetImageDetailsResponse(
                i.Id,
                i.Width,
                i.Height,
                i.Prompt,
                i.Model,
                i.Seed,
                i.SourceImageId,
                i.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(query.ImageId, details);

        _logger.LogDebug("Successfully handled get image details request.");
        return details;
    }
}
