using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.AskKitchenAssistant;

public sealed class KitchenAssistantPromptTests
{
    private static Recipe BuildRecipe(string notes = "Best served warm.")
    {
        return Recipe
            .Create(
                Guid.NewGuid(),
                "Chocolate Cake",
                null,
                [new Step("Mix the batter."), new Step("Bake for 30 minutes.")],
                [
                    new Ingredient(1.5m, "cups", "flour"),
                    new Ingredient(0m, "pinch", "salt", isOptional: true),
                ],
                notes
            )
            .State;
    }

    [Fact]
    public void Compose_WhenCalled_ShouldRenderInstructionsAndRecipeSections()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var prompt = KitchenAssistantPrompt.Compose(recipe);

        // Assert
        prompt.ShouldBe(
            KitchenAssistantPrompt.Instructions
                + """


                # Recipe: Chocolate Cake

                ## Ingredients

                1. 1.5 cups flour
                2. pinch salt (optional)

                ## Steps

                1. Mix the batter.
                2. Bake for 30 minutes.

                ## Notes

                Best served warm.
                """
        );
    }

    [Fact]
    public void Compose_WhenNotesAreEmpty_ShouldOmitNotesSection()
    {
        // Arrange
        var recipe = BuildRecipe(notes: string.Empty);

        // Act
        var prompt = KitchenAssistantPrompt.Compose(recipe);

        // Assert
        prompt.ShouldNotContain("## Notes");
        prompt.ShouldEndWith("2. Bake for 30 minutes.");
    }

    [Fact]
    public void Compose_WhenRecipeIsLarge_ShouldCapItsSectionsWithinTheContextBudgetWithEveryTool()
    {
        // Arrange
        var recipe = Recipe
            .Create(
                Guid.NewGuid(),
                new string('t', 200),
                null,
                [
                    .. Enumerable
                        .Range(0, 100)
                        .Select(i => new Step($"Step {i} " + new string('s', 1990))),
                ],
                [
                    .. Enumerable
                        .Range(0, 100)
                        .Select(i => new Ingredient(12.25m, "tablespoons", $"ingredient {i} name")),
                ],
                new string('n', 10_000)
            )
            .State;

        // Act
        var prompt = KitchenAssistantPrompt.Compose(recipe);

        // Assert
        prompt.ShouldContain("1. 12.25 tablespoons ingredient 0 name");
        prompt.ShouldContain("1. Step 0 ");
        prompt.ShouldContain("(99 more not shown)");
        prompt.ShouldMatch(@"\(\d+ more not shown\)[\s\S]*\(99 more not shown\)");
        prompt.ShouldNotContain(new string('n', KitchenAssistantPrompt.MaxNotesLength + 1));

        AssistantPromptBudget
            .Measure(prompt, KitchenAssistantToolFactory.All())
            .ShouldBeLessThanOrEqualTo(AssistantPromptBudget.Characters);
    }
}
