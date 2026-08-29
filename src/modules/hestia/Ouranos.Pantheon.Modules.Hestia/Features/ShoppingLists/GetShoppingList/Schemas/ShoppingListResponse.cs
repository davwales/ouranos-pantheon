using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;

public sealed record ShoppingListResponse(
    IReadOnlyList<Id<Recipe>> RecipeIds,
    IReadOnlyList<ConsolidatedIngredientResponse> ConsolidatedIngredients,
    IReadOnlyList<ManualItemResponse> ManualItems,
    IReadOnlyList<string> CheckedItemIds
);
