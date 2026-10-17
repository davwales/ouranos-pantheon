using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images.PurgeImages;

/// <summary>
/// Deletes generated images no saved conversation references once they are older than the
/// unsaved retention, so unsaved chats, dropped replies and deleted conversations leave no images
/// behind.
/// </summary>
public sealed class PurgeImagesJob(
    ILogger<PurgeImagesJob> logger,
    IDbContextFactory<HermesDbContext> dbContextFactory,
    IOptions<HermesOptions> options
)
{
    private readonly ILogger<PurgeImagesJob> _logger = Guard.Against.Null(logger);
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory = Guard.Against.Null(
        dbContextFactory
    );
    private readonly IOptions<HermesOptions> _options = Guard.Against.Null(options);

    [TickerFunction(nameof(PurgeImagesJob), "0 15 * * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting purge of unreferenced generated images.");
        cancellationToken.ThrowIfCancellationRequested();

        var cutoff = DateTimeOffset.UtcNow - _options.Value.ImageGeneration.UnsavedRetention;
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // A set-based delete, so image bytes never leave the database.
        var deleted = await Expired(dbContext, cutoff).ExecuteDeleteAsync(cancellationToken);
        _logger.LogDebug("Purged {Count} unreferenced generated images.", deleted);
    }

    internal static IQueryable<GeneratedImage> Expired(
        HermesDbContext dbContext,
        DateTimeOffset cutoff
    )
    {
        return dbContext.GeneratedImages.Where(i =>
            i.CreatedAt < cutoff
            && !dbContext.MessageParts.OfType<ImageMessagePart>().Any(p => p.ImageId == i.Id)
        );
    }
}
