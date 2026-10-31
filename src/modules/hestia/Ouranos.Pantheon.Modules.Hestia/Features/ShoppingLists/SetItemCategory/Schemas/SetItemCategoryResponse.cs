using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory.Schemas;

public sealed record SetItemCategoryResponse(string Name, ShoppingCategory? Category);
