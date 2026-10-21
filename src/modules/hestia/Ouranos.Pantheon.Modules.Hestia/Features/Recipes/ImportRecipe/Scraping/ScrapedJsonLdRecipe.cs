using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;

public sealed record ScrapedJsonLdRecipe(string Title, string RawJson, RecipeDetails Details);
