using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

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
                [new Ingredient(1.5m, "cups", "flour"), new Ingredient(0m, "pinch", "salt")],
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

                - 1.5 cups flour
                - pinch salt

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
}
