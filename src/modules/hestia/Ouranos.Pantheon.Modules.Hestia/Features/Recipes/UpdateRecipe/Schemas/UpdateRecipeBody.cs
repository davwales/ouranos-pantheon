namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe.Schemas;

public sealed record UpdateRecipeBody(
    string Title,
    string? SourceUrl,
    IReadOnlyList<StepInput> Steps,
    IReadOnlyList<IngredientInput> Ingredients,
    string Notes
);
