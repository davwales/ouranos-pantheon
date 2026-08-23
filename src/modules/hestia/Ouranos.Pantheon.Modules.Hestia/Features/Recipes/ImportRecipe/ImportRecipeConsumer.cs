using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeConsumer(ILogger<ImportRecipeConsumer> logger)
    : IPantheonHandler<ImportRecipeRequested>
{
    private readonly ILogger<ImportRecipeConsumer> _logger = Guard.Against.Null(logger);

    public Task Handle(ImportRecipeRequested message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Processing import recipe request '{@message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug(
            "Successfully processed import recipe request for recipe '{recipeId}'. Scraping is not yet implemented.",
            message.RecipeId
        );
        return Task.CompletedTask;
    }
}
