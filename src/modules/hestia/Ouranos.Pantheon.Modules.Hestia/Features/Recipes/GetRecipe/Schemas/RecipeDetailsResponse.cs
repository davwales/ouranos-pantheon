namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe.Schemas;

public sealed record RecipeDetailsResponse(
    string? Yield,
    int? PrepMinutes,
    int? CookMinutes,
    int? TotalMinutes,
    string? ImageUrl
);
