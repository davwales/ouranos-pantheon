using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;

public sealed record ManualItemResponse(Guid Id, string Text, ShoppingCategory? Category);
