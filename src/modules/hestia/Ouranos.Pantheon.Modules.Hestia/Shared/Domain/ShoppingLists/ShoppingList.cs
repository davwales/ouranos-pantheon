using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

public sealed class ShoppingList
{
    public static readonly Guid FixedId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; set; } = FixedId;
    public List<Id<Recipe>> RecipeIds { get; set; } = [];
    public List<ManualItem> ManualItems { get; set; } = [];
    public List<string> CheckedItemIds { get; set; } = [];
}
