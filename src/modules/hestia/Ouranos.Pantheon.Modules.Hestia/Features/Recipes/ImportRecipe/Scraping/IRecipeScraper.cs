namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;

public interface IRecipeScraper
{
    Task<ScrapedJsonLdRecipe?> ScrapeAsync(
        string url,
        CancellationToken cancellationToken = default
    );
}
