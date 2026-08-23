using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory.Schemas;

public sealed record GetRecipeHistoryResponse(
    Id<Recipe> RecipeId,
    IReadOnlyList<RecipeHistoryEventResponse> Events
);
