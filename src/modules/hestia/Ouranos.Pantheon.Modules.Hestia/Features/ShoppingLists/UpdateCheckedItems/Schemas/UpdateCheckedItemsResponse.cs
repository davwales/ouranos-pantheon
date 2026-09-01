namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems.Schemas;

public sealed record UpdateCheckedItemsResponse(IReadOnlyList<string> CheckedItemIds);
