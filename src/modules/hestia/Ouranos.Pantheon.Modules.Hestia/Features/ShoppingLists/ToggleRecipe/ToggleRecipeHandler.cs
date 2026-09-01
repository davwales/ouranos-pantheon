using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe;

public sealed class ToggleRecipeHandler(
    ILogger<ToggleRecipeHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<ToggleRecipeInput, ToggleRecipeResponse>
{
    private readonly ILogger<ToggleRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<ToggleRecipeResponse> Handle(
        ToggleRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle toggle recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (!command.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(command.RecipeId, (Recipe?)null);
        }

        using var session = _store.LightweightSession();
        var recipe = await session.LoadAsync<Recipe>(streamId, cancellationToken);
        Guard.Against.NotFound(command.RecipeId, recipe);

        var list =
            await session.LoadAsync<ShoppingList>(ShoppingList.FixedId, cancellationToken)
            ?? new ShoppingList();

        var wasInList = list.RecipeIds.Contains(command.RecipeId);
        if (wasInList)
        {
            list.RecipeIds.Remove(command.RecipeId);
        }
        else
        {
            list.RecipeIds.Add(command.RecipeId);
        }

        session.Store(list);
        await session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully toggled recipe '{recipeId}' membership to '{isInList}'.",
            command.RecipeId,
            !wasInList
        );
        return new ToggleRecipeResponse(command.RecipeId, !wasInList);
    }
}
