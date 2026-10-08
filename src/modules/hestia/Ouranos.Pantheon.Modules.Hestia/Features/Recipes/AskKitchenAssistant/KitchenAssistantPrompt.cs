using System.Globalization;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;

internal static class KitchenAssistantPrompt
{
    public const string Instructions = """
        You are a friendly, practical kitchen assistant helping a home cook with the recipe below.
        Answer questions about this recipe: substitutions, scaling, timing, techniques, equipment,
        storage, and troubleshooting. Keep answers concise and use markdown lists where they help.
        When a question depends on details the recipe does not give, say so and suggest a sensible
        default. Never invent ingredients or steps and present them as part of the recipe.
        """;

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
        return $"""
            ## Ingredients

            {string.Join("\n", recipe.Ingredients.Select(FormatIngredient))}
            """;
    }

    private static string ComposeSteps(Recipe recipe)
    {
        return $"""
            ## Steps

            {string.Join("\n", recipe.Steps.Select((step, i) => $"{i + 1}. {step.Text}"))}
            """;
    }

    private static string ComposeNotes(Recipe recipe)
    {
        if (string.IsNullOrWhiteSpace(recipe.Notes))
        {
            return string.Empty;
        }

        return $"""
            ## Notes

            {recipe.Notes}
            """;
    }

    private static string FormatIngredient(Ingredient ingredient)
    {
        if (ingredient.Quantity <= 0)
        {
            return $"- {ingredient.Unit} {ingredient.Name}";
        }

        var quantity = ingredient.Quantity.ToString("0.##", CultureInfo.InvariantCulture);
        return $"- {quantity} {ingredient.Unit} {ingredient.Name}";
    }
}
