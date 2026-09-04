using Ardalis.GuardClauses;
using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList;

public sealed class GetShoppingListHandler(
    ILogger<GetShoppingListHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<GetShoppingListInput, ShoppingListResponse>
{
    private readonly ILogger<GetShoppingListHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<ShoppingListResponse> Handle(
        GetShoppingListInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get shopping list query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        using var session = _store.QuerySession();
        var list = await LoadShoppingList(session, cancellationToken);
        var recipes = await LoadRecipes(session, list, cancellationToken);
        var lines = ConsolidateIngredients(recipes);
        var checkedItemIds = PruneStaleCheckedItemIds(list, lines);

        _logger.LogDebug("Successfully handled get shopping list request.");
        return new ShoppingListResponse(
            [.. list.RecipeIds],
            ProjectRecipes(list, recipes),
            lines,
            [.. list.ManualItems.Select(i => new ManualItemResponse(i.Id, i.Text))],
            checkedItemIds
        );
    }

    private static async Task<ShoppingList> LoadShoppingList(
        IQuerySession session,
        CancellationToken cancellationToken
    )
    {
        return await session.LoadAsync<ShoppingList>(ShoppingList.FixedId, cancellationToken)
            ?? new ShoppingList();
    }

    private static async Task<IReadOnlyList<Recipe>> LoadRecipes(
        IQuerySession session,
        ShoppingList list,
        CancellationToken cancellationToken
    )
    {
        var streamIds = list.RecipeIds.Select(r => r.GetStreamId()).ToArray();
        if (streamIds.Length == 0)
        {
            return [];
        }

        return await session.LoadManyAsync<Recipe>(cancellationToken, streamIds);
    }

    private static List<ConsolidatedIngredientResponse> ConsolidateIngredients(
        IReadOnlyList<Recipe> recipes
    )
    {
        var consolidated = new Dictionary<string, (string Name, string Unit, decimal Quantity)>(
            StringComparer.Ordinal
        );

        foreach (var recipe in recipes)
        {
            if (recipe is null)
            {
                continue;
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                var key = ShoppingListNormalizer.RecipeLineKey(
                    ShoppingListNormalizer.Normalize(ingredient.Name),
                    ShoppingListNormalizer.Normalize(ingredient.Unit)
                );
                if (consolidated.TryGetValue(key, out var existing))
                {
                    consolidated[key] = (
                        existing.Name,
                        existing.Unit,
                        existing.Quantity + ingredient.Quantity
                    );
                }
                else
                {
                    consolidated[key] = (ingredient.Name, ingredient.Unit, ingredient.Quantity);
                }
            }
        }

        return
        [
            .. consolidated
                .Select(kv => new ConsolidatedIngredientResponse(
                    kv.Key,
                    kv.Value.Name,
                    kv.Value.Unit,
                    kv.Value.Quantity
                ))
                .OrderBy(l => l.Name, StringComparer.Ordinal)
                .ThenBy(l => l.Unit, StringComparer.Ordinal),
        ];
    }

    private static List<ShoppingListRecipeResponse> ProjectRecipes(
        ShoppingList list,
        IReadOnlyList<Recipe> recipes
    )
    {
        var titles = recipes.Where(r => r is not null).ToDictionary(r => r.Id, r => r.Title);

        var responses = new List<ShoppingListRecipeResponse>();
        foreach (var recipeId in list.RecipeIds)
        {
            if (titles.TryGetValue(recipeId.GetStreamId(), out var title))
            {
                responses.Add(new ShoppingListRecipeResponse(recipeId, title));
            }
        }

        return responses;
    }

    private static List<string> PruneStaleCheckedItemIds(
        ShoppingList list,
        IReadOnlyList<ConsolidatedIngredientResponse> lines
    )
    {
        var validKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in lines)
        {
            validKeys.Add(line.Id);
        }

        foreach (var item in list.ManualItems)
        {
            validKeys.Add(ShoppingListNormalizer.ManualLineKey(item.Id));
        }

        return [.. list.CheckedItemIds.Where(validKeys.Contains)];
    }
}
