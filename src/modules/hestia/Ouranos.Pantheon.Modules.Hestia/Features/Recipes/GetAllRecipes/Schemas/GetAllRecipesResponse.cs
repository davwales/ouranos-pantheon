using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes.Schemas;

public sealed record GetAllRecipesResponse(
    Id<Recipe> Id,
    string Title,
    string? SourceUrl,
    DateTimeOffset CreatedAt,
    int IngredientCount,
    int StepCount,
    RecipeImportStatus ImportStatus
);
