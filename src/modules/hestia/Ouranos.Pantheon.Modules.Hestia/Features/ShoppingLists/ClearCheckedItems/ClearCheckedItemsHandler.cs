using Ardalis.GuardClauses;
using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems;

/// <summary>
/// Removes what has been bought: checked manual items, and recipes whose every ingredient line is
/// checked. A partly bought recipe stays, with its checked lines still checked.
/// </summary>
public sealed class ClearCheckedItemsHandler(
    ILogger<ClearCheckedItemsHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<ClearCheckedItemsInput, ClearCheckedItemsResponse>
{
    private readonly ILogger<ClearCheckedItemsHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<ClearCheckedItemsResponse> Handle(
        ClearCheckedItemsInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle clear checked items command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        using var session = _store.LightweightSession();
        var list =
            await session.LoadAsync<ShoppingList>(ShoppingList.FixedId, cancellationToken)
            ?? new ShoppingList();
        var checkedIds = list.CheckedItemIds.ToHashSet(StringComparer.Ordinal);

        var removedManualItemIds = list
            .ManualItems.Where(i => checkedIds.Contains(ShoppingListNormalizer.ManualLineKey(i.Id)))
            .Select(i => i.Id)
            .ToList();
        list.ManualItems.RemoveAll(i => removedManualItemIds.Contains(i.Id));

        var recipes = await LoadRecipes(session, list, cancellationToken);
        var boughtStreamIds = recipes
            .Where(r => IsFullyChecked(r, checkedIds))
            .Select(r => r.Id)
            .ToHashSet();
        var removedRecipeIds = list
            .RecipeIds.Where(id => boughtStreamIds.Contains(id.GetStreamId()))
            .ToList();
        list.RecipeIds.RemoveAll(removedRecipeIds.Contains);

        var remainingLineIds = ShoppingListConsolidator
            .Consolidate(recipes.Where(r => !boughtStreamIds.Contains(r.Id)))
            .Select(l => l.Id)
            .ToHashSet(StringComparer.Ordinal);
        list.CheckedItemIds = [.. list.CheckedItemIds.Where(remainingLineIds.Contains)];

        session.Store(list);
        await session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully cleared {manualItemCount} manual items and {recipeCount} recipes.",
            removedManualItemIds.Count,
            removedRecipeIds.Count
        );
        return new ClearCheckedItemsResponse(removedManualItemIds, removedRecipeIds);
    }

    private static async Task<IReadOnlyList<Recipe>> LoadRecipes(
        IDocumentSession session,
        ShoppingList list,
        CancellationToken cancellationToken
    )
    {
        var streamIds = list.RecipeIds.Select(r => r.GetStreamId()).ToArray();
        if (streamIds.Length == 0)
        {
            return [];
        }

        var recipes = await session.LoadManyAsync<Recipe>(cancellationToken, streamIds);
        return [.. recipes.Where(r => r is not null)];
    }

    private static bool IsFullyChecked(Recipe recipe, HashSet<string> checkedIds)
    {
        var lineIds = ShoppingListConsolidator.LineIds(recipe);
        return lineIds.Count > 0 && lineIds.All(checkedIds.Contains);
    }
}
