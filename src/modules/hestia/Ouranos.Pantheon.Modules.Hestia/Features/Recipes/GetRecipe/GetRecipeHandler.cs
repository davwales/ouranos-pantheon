using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe;

public sealed class GetRecipeHandler(ILogger<GetRecipeHandler> logger, IHestiaMartenStore store)
    : IPantheonHandler<GetRecipeInput, GetRecipeResponse>
{
    private readonly ILogger<GetRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<GetRecipeResponse> Handle(
        GetRecipeInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recipe query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        if (!query.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(query.RecipeId, (Recipe?)null);
        }

        using var session = _store.QuerySession();
        var recipe = await session.LoadAsync<Recipe>(streamId, cancellationToken);
        Guard.Against.NotFound(query.RecipeId, recipe);

        _logger.LogDebug(
            "Successfully handled get recipe request for recipe '{recipeId}'.",
            query.RecipeId
        );
        return new GetRecipeResponse(
            recipe.RecipeId,
            recipe.Title,
            recipe.SourceUrl,
            recipe.Instructions,
            [.. recipe.Ingredients.Select(i => new IngredientResponse(i.Quantity, i.Unit, i.Name))],
            recipe.Notes,
            recipe.CreatedAt
        );
    }
}
