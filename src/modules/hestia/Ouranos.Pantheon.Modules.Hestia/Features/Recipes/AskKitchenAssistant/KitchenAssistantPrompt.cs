using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;

internal static class KitchenAssistantPrompt
{
    public const string Instructions = """
        You are a friendly, practical kitchen assistant helping a home cook with the recipe below.
        Answer questions about this recipe: substitutions, scaling, timing, techniques, equipment,
        storage, and troubleshooting. Keep answers concise.
        When a question depends on details the recipe does not give, say so and suggest a sensible
        default. Never invent ingredients or steps and present them as part of the recipe.
        Tools can propose changes to the recipe: a note, an ingredient to add, change, mark
        optional or remove, or a reworded step. Refer to ingredients and steps by their numbers.
        """;

    internal const int MaxIngredientCharacters = 1500;

    internal const int MaxStepCharacters = 2500;

    internal const int MaxNotesLength = 800;

    public static string Compose(Recipe recipe)
    {
        string[] sections =
        [
            Instructions,
            ComposeTitle(recipe),
            ComposeIngredients(recipe),
            ComposeSteps(recipe),
            ComposeNotes(recipe),
        ];

        return string.Join("\n\n", sections.Where(section => section.Length > 0));
    }

    private static string ComposeTitle(Recipe recipe)
    {
        return $"# Recipe: {recipe.Title}";
    }

    private static string ComposeIngredients(Recipe recipe)
    {
        var lines = TakeWithin(
            recipe.Ingredients.Select(
                (ingredient, i) =>
                    $"{i + 1}. {KitchenRecipes.FormatIngredient(KitchenRecipes.ToProposed(ingredient))}"
            ),
            MaxIngredientCharacters
        );

        return $"""
            ## Ingredients

            {string.Join("\n", lines)}{FormatOmitted(recipe.Ingredients.Count, lines.Count)}
            """;
    }

    private static string ComposeSteps(Recipe recipe)
    {
        var lines = TakeWithin(
            recipe.Steps.Select((step, i) => $"{i + 1}. {step.Text}"),
            MaxStepCharacters
        );

        return $"""
            ## Steps

            {string.Join("\n", lines)}{FormatOmitted(recipe.Steps.Count, lines.Count)}
            """;
    }

    // Always keeps the first line, so a single long step still shows.
    private static List<string> TakeWithin(IEnumerable<string> lines, int maxCharacters)
    {
        var kept = new List<string>();
        var used = 0;

        foreach (var line in lines)
        {
            used += line.Length + 1;
            if (kept.Count > 0 && used > maxCharacters)
            {
                break;
            }

            kept.Add(line);
        }

        return kept;
    }

    private static string FormatOmitted(int count, int shown)
    {
        return count > shown ? $"\n({count - shown} more not shown)" : "";
    }

    private static string ComposeNotes(Recipe recipe)
    {
        if (string.IsNullOrWhiteSpace(recipe.Notes))
        {
            return string.Empty;
        }

        var notes =
            recipe.Notes.Length <= MaxNotesLength
                ? recipe.Notes
                : $"{recipe.Notes[..MaxNotesLength]}…";

        return $"""
            ## Notes

            {notes}
            """;
    }
}
