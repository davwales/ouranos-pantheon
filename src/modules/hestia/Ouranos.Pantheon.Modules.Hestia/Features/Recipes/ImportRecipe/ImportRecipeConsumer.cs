using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeConsumer(
    ILogger<ImportRecipeConsumer> logger,
    IRecipeScraper scraper,
    IRecipeExtractor extractor,
    IHestiaMartenStore store
) : IPantheonHandler<ImportRecipeRequested>
{
    private readonly ILogger<ImportRecipeConsumer> _logger = Guard.Against.Null(logger);
    private readonly IRecipeScraper _scraper = Guard.Against.Null(scraper);
    private readonly IRecipeExtractor _extractor = Guard.Against.Null(extractor);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task Handle(
        ImportRecipeRequested message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Processing import recipe request '{@message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        if (!message.RecipeId.TryGetStreamId(out var recipeId))
        {
            _logger.LogWarning(
                "Skipped importing recipe '{recipeId}' from '{url}': the recipe id is not a valid stream id.",
                message.RecipeId,
                message.Url
            );
            return;
        }

        using var session = _store.LightweightSession();
        var recipe = await session.LoadAsync<Recipe>(recipeId, cancellationToken);
        if (recipe is null)
        {
            _logger.LogWarning(
                "Skipped importing recipe '{recipeId}' from '{url}': the recipe does not exist.",
                message.RecipeId,
                message.Url
            );
            return;
        }

        // A redelivered message, or one for an import the sweeper already failed, must not run the
        // model again.
        if (recipe.ImportStatus != RecipeImportStatus.Importing)
        {
            _logger.LogDebug(
                "Skipped importing recipe '{recipeId}' from '{url}': the recipe is not importing ({status}).",
                message.RecipeId,
                message.Url,
                recipe.ImportStatus
            );
            return;
        }

        var result = await AttemptImportAsync(message, recipe, cancellationToken);

        session.Events.Append(recipeId, [.. result.Events]);
        await session.SaveChangesAsync(cancellationToken);

        if (result.Events[0] is RecipeImportFailed failure)
        {
            _logger.LogWarning(
                "Import of recipe '{recipeId}' from '{url}' failed: {reason}.",
                message.RecipeId,
                message.Url,
                failure.Reason
            );
            return;
        }

        _logger.LogDebug(
            "Successfully imported recipe '{recipeId}' from '{url}'.",
            message.RecipeId,
            message.Url
        );
    }

    private async Task<OperationResult<Recipe>> AttemptImportAsync(
        ImportRecipeRequested message,
        Recipe recipe,
        CancellationToken cancellationToken
    )
    {
        ScrapedJsonLdRecipe? scraped = null;
        try
        {
            scraped = await _scraper.ScrapeAsync(message.Url, cancellationToken);
            if (scraped is null)
            {
                return recipe.FailImport("The page contains no usable recipe metadata.");
            }

            var extracted = await _extractor.ExtractAsync(scraped.RawJson, cancellationToken);
            if (extracted is null)
            {
                return recipe.FailImport("Recipe extraction produced no usable result.");
            }

            var mapped = ExtractedRecipeMapper.TryMap(extracted);
            if (mapped is null)
            {
                return recipe.FailImport("The extracted recipe failed validation.");
            }

            return recipe.CompleteImport(
                mapped.Title,
                mapped.Steps,
                mapped.Ingredients,
                mapped.Notes,
                scraped.Details
            );
        }
        catch (Exception ex) when (!IsCancellation(ex, cancellationToken))
        {
            _logger.LogError(
                ex,
                "Failed to import recipe '{recipeId}' from '{url}'.",
                message.RecipeId,
                message.Url
            );
            return recipe.FailImport(FailureReason(ex, downloaded: scraped is not null));
        }
    }

    // HttpClient and model timeouts also throw TaskCanceledException, so only the caller's token
    // marks a real cancellation; a timeout must end the import, not be retried and dead-lettered.
    private static bool IsCancellation(Exception ex, CancellationToken cancellationToken)
    {
        return ex is OperationCanceledException && cancellationToken.IsCancellationRequested;
    }

    // Exception text is not fit for users and lives forever in the event stream.
    private static string FailureReason(Exception ex, bool downloaded)
    {
        return (ex, downloaded) switch
        {
            (OperationCanceledException, false) => "The recipe site took too long to respond.",
            (OperationCanceledException, true) => "Reading the recipe took too long.",
            (_, false) => "The recipe page could not be loaded.",
            _ => "The recipe could not be read from the page.",
        };
    }
}
