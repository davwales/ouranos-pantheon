namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;

public sealed record ConsolidatedIngredientResponse(
    string Id,
    string Name,
    string Unit,
    decimal Quantity
);
