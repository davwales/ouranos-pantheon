using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;

public sealed record ConsolidatedIngredientResponse(
    string Id,
    string Name,
    string Unit,
    decimal Quantity,
    ShoppingCategory? Category
);
