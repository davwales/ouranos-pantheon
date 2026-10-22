using System.Linq.Expressions;
using Ardalis.GuardClauses;
using JasperFx;
using Marten;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.SweepStaleImports;

/// <summary>
/// Fails imports that have been in progress longer than the stale timeout, so a lost message or a
/// crash between saving and publishing never leaves a recipe importing forever.
/// </summary>
public sealed class SweepStaleImportsJob(
    ILogger<SweepStaleImportsJob> logger,
    IHestiaMartenStore store,
    IOptions<HestiaOptions> options
)
{
    private readonly ILogger<SweepStaleImportsJob> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);
    private readonly IOptions<HestiaOptions> _options = Guard.Against.Null(options);

    [TickerFunction(nameof(SweepStaleImportsJob), "0 */5 * * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting sweep of stale recipe imports.");
        cancellationToken.ThrowIfCancellationRequested();

        var cutoff = DateTimeOffset.UtcNow - _options.Value.RecipeImport.StaleImportTimeout;

        IReadOnlyList<Guid> staleIds;
        using (var query = _store.QuerySession())
        {
            staleIds = await query
                .Query<Recipe>()
                .Where(Stale(cutoff))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);
        }

        var failed = 0;
        foreach (var recipeId in staleIds)
        {
            if (await TryFailAsync(recipeId, cutoff, cancellationToken))
            {
                failed++;
            }
        }

        _logger.LogDebug("Marked {Count} stale recipe imports as failed.", failed);
    }

    internal async Task<bool> TryFailAsync(
        Guid recipeId,
        DateTimeOffset cutoff,
        CancellationToken cancellationToken
    )
    {
        using var session = _store.LightweightSession();
        var stream = await session.Events.FetchForWriting<Recipe>(recipeId, cancellationToken);
        var recipe = stream.Aggregate;
        if (recipe is null || !Stale(cutoff).Compile()(recipe))
        {
            return false;
        }

        stream.AppendMany([.. recipe.FailImport(RecipeImportFailureReasons.Stale).Events]);

        try
        {
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            // The import finished (or was reimported) since it was read; leave it alone.
            return false;
        }

        _logger.LogWarning("Marked stale import of recipe '{recipeId}' as failed.", recipeId);
        return true;
    }

    // Recipes imported before ImportStartedAt existed have no start time; any of those still
    // importing are long stale.
    internal static Expression<Func<Recipe, bool>> Stale(DateTimeOffset cutoff)
    {
        return r =>
            r.ImportStatus == RecipeImportStatus.Importing
            && (r.ImportStartedAt == null || r.ImportStartedAt < cutoff);
    }
}
