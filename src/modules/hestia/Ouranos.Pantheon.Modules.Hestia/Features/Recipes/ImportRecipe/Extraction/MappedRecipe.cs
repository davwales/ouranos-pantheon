using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;

internal sealed record MappedRecipe(
    string Title,
    string Notes,
    List<Step> Steps,
    List<Ingredient> Ingredients
);
