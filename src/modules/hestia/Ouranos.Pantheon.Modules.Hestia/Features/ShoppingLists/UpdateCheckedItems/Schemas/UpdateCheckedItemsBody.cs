namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems.Schemas;

public sealed record UpdateCheckedItemsBody(IReadOnlyList<string> CheckedItemIds);
