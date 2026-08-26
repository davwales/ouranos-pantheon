using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;

internal static class ExtractedRecipeMapper
{
    private const int MaxTitleLength = 200;
    private const int MaxNotesLength = 10_000;
    private const int MaxStepLength = 2_000;
    private const int MaxNameLength = 200;
    private const int MaxUnitLength = 50;
    private const int MaxItems = 100;

    public static MappedRecipe? TryMap(ExtractedRecipe extracted)
    {
        var title = Truncate((extracted.Title ?? string.Empty).Trim(), MaxTitleLength);
        if (title.Length == 0)
        {
            return null;
        }

        var steps = MapSteps(extracted.Steps);
        if (steps.Count == 0)
        {
            return null;
        }

        var ingredients = MapIngredients(extracted.Ingredients);
        if (ingredients.Count == 0)
        {
            return null;
        }

        var notes = extracted.Description is null
            ? string.Empty
            : Truncate(extracted.Description.Trim(), MaxNotesLength);

        return new MappedRecipe(title, notes, steps, ingredients);
    }

    private static List<Step> MapSteps(List<string> rawSteps)
    {
        var steps = new List<Step>();
        foreach (var rawStep in rawSteps)
        {
            if (steps.Count >= MaxItems)
            {
                break;
            }

            var text = Truncate((rawStep ?? string.Empty).Trim(), MaxStepLength);
            if (text.Length == 0)
            {
                continue;
            }

            steps.Add(new Step(text));
        }

        return steps;
    }

    private static List<Ingredient> MapIngredients(List<ExtractedIngredient> rawIngredients)
    {
        var ingredients = new List<Ingredient>();
        foreach (var rawIngredient in rawIngredients)
        {
            if (ingredients.Count >= MaxItems)
            {
                break;
            }

            var name = Truncate((rawIngredient.Name ?? string.Empty).Trim(), MaxNameLength);
            if (name.Length == 0)
            {
                continue;
            }

            ingredients.Add(
                new Ingredient(
                    ResolveQuantity(rawIngredient.Quantity),
                    ResolveUnit(rawIngredient.Unit),
                    name
                )
            );
        }

        return ingredients;
    }

    private static decimal ResolveQuantity(decimal? quantity)
    {
        if (quantity is null || quantity < 0)
        {
            return 0m;
        }

        return quantity.Value;
    }

    private static string ResolveUnit(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            return "whole";
        }

        var normalized = unit.Trim().ToLowerInvariant();
        return Truncate(normalized, MaxUnitLength);
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
