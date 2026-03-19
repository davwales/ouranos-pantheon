namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;

public sealed record GetAllRecipesInput(
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
);
