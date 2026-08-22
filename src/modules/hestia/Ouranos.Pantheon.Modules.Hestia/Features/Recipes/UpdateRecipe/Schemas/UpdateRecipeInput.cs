using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe.Schemas;

public sealed record UpdateRecipeInput(
    Id<Recipe> RecipeId,
    string Title,
    string? SourceUrl,
    IReadOnlyList<StepInput> Steps,
    IReadOnlyList<IngredientInput> Ingredients,
    string Notes
);
