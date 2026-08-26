namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;

public sealed record ExtractedIngredient(decimal? Quantity, string? Unit, string Name = "");
