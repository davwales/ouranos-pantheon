using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems.Schemas;

public sealed record ClearCheckedItemsResponse(
    IReadOnlyList<Guid> RemovedManualItemIds,
    IReadOnlyList<Id<Recipe>> RemovedRecipeIds
);
