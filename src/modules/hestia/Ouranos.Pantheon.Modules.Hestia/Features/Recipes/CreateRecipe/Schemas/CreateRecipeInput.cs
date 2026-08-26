namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe.Schemas;

public sealed record CreateRecipeInput(
    string Title,
    string? SourceUrl,
    IReadOnlyList<StepInput> Steps,
    IReadOnlyList<IngredientInput> Ingredients,
    string Notes
);
