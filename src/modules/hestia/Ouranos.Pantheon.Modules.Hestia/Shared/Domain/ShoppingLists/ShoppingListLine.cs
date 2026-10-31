namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

public sealed record ShoppingListLine(
    string Id,
    string NameKey,
    string Name,
    string Unit,
    decimal Quantity
);
