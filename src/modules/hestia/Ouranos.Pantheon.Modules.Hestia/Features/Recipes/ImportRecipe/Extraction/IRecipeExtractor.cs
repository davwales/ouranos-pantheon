using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;

public interface IRecipeExtractor
{
    Task<ExtractedRecipe?> ExtractAsync(
        string recipeJsonLd,
        CancellationToken cancellationToken = default
    );
}
