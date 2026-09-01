namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems.Schemas;

public sealed record UpdateCheckedItemsInput(IReadOnlyList<string> CheckedItemIds);
