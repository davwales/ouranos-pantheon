using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe.Schemas;

public sealed record GetRecipeResponse(
    Id<Recipe> Id,
    string Title,
    string? SourceUrl,
    IReadOnlyList<StepResponse> Steps,
    IReadOnlyList<IngredientResponse> Ingredients,
    string Notes,
    DateTimeOffset CreatedAt
);
