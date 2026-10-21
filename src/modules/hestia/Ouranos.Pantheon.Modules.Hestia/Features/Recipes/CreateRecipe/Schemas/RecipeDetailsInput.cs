namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe.Schemas;

public sealed record RecipeDetailsInput(
    string? Yield,
    int? PrepMinutes,
    int? CookMinutes,
    int? TotalMinutes,
    string? ImageUrl
);
