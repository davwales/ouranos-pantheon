using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

public static class ShoppingListConsolidator
{
    private const string UnquantifiedMeasure = "unquantified";
    private const int ConvertedDecimals = 3;

    public static IReadOnlyList<ShoppingListLine> Consolidate(IEnumerable<Recipe> recipes)
    {
        var lines = new Dictionary<string, ShoppingListLine>(StringComparer.Ordinal);
        var converted = new Dictionary<string, (decimal BaseAmount, MeasurementUnit Largest)>(
            StringComparer.Ordinal
        );

        foreach (var ingredient in recipes.SelectMany(PurchasableIngredients))
        {
            var nameKey = ShoppingListNormalizer.NameKey(ingredient.Name);
            var unit = ConvertibleUnit(ingredient);
            var id = LineId(nameKey, ingredient, unit);
            var line = lines.GetValueOrDefault(id);

            if (unit is not null)
            {
                var amount = ingredient.Quantity * unit.BaseFactor;
                converted[id] = converted.TryGetValue(id, out var total)
                    ? (
                        total.BaseAmount + amount,
                        unit.BaseFactor > total.Largest.BaseFactor ? unit : total.Largest
                    )
                    : (amount, unit);
            }

            lines[id] = line is null
                ? new ShoppingListLine(
                    id,
                    nameKey,
                    ingredient.Name.Trim(),
                    ShoppingListNormalizer.Normalize(ingredient.Unit),
                    ingredient.Quantity
                )
                : line with
                {
                    Quantity = line.Quantity + ingredient.Quantity,
                };
        }

        // Mixed units of one dimension are shown in the largest unit the recipes used (3 tsp + 1 tbsp -> 2 tbsp).
        foreach (var (id, total) in converted)
        {
            lines[id] = lines[id] with
            {
                Unit = total.Largest.Symbol,
                Quantity = Math.Round(
                    total.BaseAmount / total.Largest.BaseFactor,
                    ConvertedDecimals
                ),
            };
        }

        return
        [
            .. lines
                .Values.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(l => l.Unit, StringComparer.Ordinal),
        ];
    }

    public static IReadOnlySet<string> LineIds(Recipe recipe)
    {
        return PurchasableIngredients(recipe)
            .Select(i => LineId(ShoppingListNormalizer.NameKey(i.Name), i, ConvertibleUnit(i)))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static MeasurementUnit? ConvertibleUnit(Ingredient ingredient)
    {
        return ingredient.Quantity > 0 ? MeasurementUnits.Find(ingredient.Unit) : null;
    }

    // Optional ingredients are left to the user, who can add them as manual items.
    private static IEnumerable<Ingredient> PurchasableIngredients(Recipe recipe)
    {
        return recipe.Ingredients.Where(i => !i.IsOptional);
    }

    // Quantified lines merge per unit dimension (volume, mass) or, for units that don't convert
    // (clove, can), per unit. Unquantified lines ("salt, to taste") stay separate from quantified ones.
    private static string LineId(string nameKey, Ingredient ingredient, MeasurementUnit? unit)
    {
        if (ingredient.Quantity <= 0)
        {
            return ShoppingListNormalizer.RecipeLineKey(nameKey, UnquantifiedMeasure);
        }

        var measure =
            unit?.Dimension.ToString().ToLowerInvariant()
            ?? ShoppingListNormalizer.NameKey(ingredient.Unit);
        return ShoppingListNormalizer.RecipeLineKey(nameKey, measure);
    }
}
