namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;

public sealed record ExtractedRecipe(
    string Title,
    string? Description,
    List<ExtractedIngredient> Ingredients,
    List<string> Steps
)
{
    public List<ExtractedIngredient> Ingredients { get; init; } = Ingredients ?? [];

    public List<string> Steps { get; init; } = Steps ?? [];
}
