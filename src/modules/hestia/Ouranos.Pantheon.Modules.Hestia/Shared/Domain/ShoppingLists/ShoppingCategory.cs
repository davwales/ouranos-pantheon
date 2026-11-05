using System.Text.Json.Serialization;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

// Declared in store-walk order, which is the order the list groups by. Stored by name so reordering is safe.
[JsonConverter(typeof(JsonStringEnumConverter<ShoppingCategory>))]
public enum ShoppingCategory
{
    Produce,
    Bakery,
    MeatAndSeafood,
    DairyAndEggs,
    Frozen,
    Pantry,
    SpicesAndSeasonings,
    Beverages,
    Household,
}
