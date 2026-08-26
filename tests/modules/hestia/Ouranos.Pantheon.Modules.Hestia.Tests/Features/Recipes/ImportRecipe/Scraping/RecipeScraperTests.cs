using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Security.AntiSSRF;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe.Scraping;

public sealed class RecipeScraperTests
{
    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> respond
    ) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            return Task.FromResult(_respond(request));
        }
    }

    private static RecipeScraper CreateScraper(
        Func<HttpRequestMessage, HttpResponseMessage> respond
    )
    {
        var handler = new StubHttpMessageHandler(respond);
        var httpClient = new HttpClient(handler);
        return new RecipeScraper(httpClient, Substitute.For<ILogger<RecipeScraper>>());
    }

    private static HttpResponseMessage HtmlResponse(string html)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html"),
        };
    }

    [Fact]
    public async Task ScrapeAsync_WhenPageHasRecipe_ShouldReturnScrapedJsonLdRecipe()
    {
        // Arrange
        var html =
            """<script type="application/ld+json">{"@type":"Recipe","name":"Test Recipe","recipeIngredient":["1 cup flour"],"recipeInstructions":[{"@type":"HowToStep","text":"Mix."}]}</script>""";
        var scraper = CreateScraper(_ => HtmlResponse(html));

        // Act
        var result = await scraper.ScrapeAsync(
            "https://example.com/recipe",
            CancellationToken.None
        );

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Test Recipe");
        recipe.RawJson.ShouldContain("\"recipeIngredient\"");
        recipe.RawJson.ShouldContain("\"recipeInstructions\"");
    }

    [Fact]
    public async Task ScrapeAsync_WhenPageHasNoRecipeMetadata_ShouldReturnNull()
    {
        // Arrange
        var html = """<html><body><h1>Not a recipe</h1></body></html>""";
        var scraper = CreateScraper(_ => HtmlResponse(html));

        // Act
        var result = await scraper.ScrapeAsync("https://example.com/plain", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ScrapeAsync_WhenNonSuccessStatus_ShouldThrowHttpRequestException()
    {
        // Arrange
        var scraper = CreateScraper(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var act = async () =>
            await scraper.ScrapeAsync("https://example.com/missing", CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ScrapeAsync_WhenContentTypeIsNotHtml_ShouldReturnNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        var scraper = CreateScraper(_ => response);

        // Act
        var result = await scraper.ScrapeAsync(
            "https://example.com/recipe.json",
            CancellationToken.None
        );

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ScrapeAsync_WhenContentExceedsMaxSize_ShouldReturnNull()
    {
        // Arrange
        var huge = new string('a', 5_000_001);
        var scraper = CreateScraper(_ => HtmlResponse(huge));

        // Act
        var result = await scraper.ScrapeAsync("https://example.com/huge", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ScrapeAsync_WhenUrlIsNotHttp_ShouldReturnNull()
    {
        // Arrange
        var scraper = CreateScraper(_ => HtmlResponse("<html></html>"));

        // Act
        var result = await scraper.ScrapeAsync("ftp://example.com/recipe", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ScrapeAsync_WhenUrlTargetsPrivateAddress_ShouldReturnNull()
    {
        // Arrange
        var policy = new AntiSSRFPolicy(PolicyConfigOptions.ExternalOnlyLatest);
        var httpClient = new HttpClient(policy.GetHandler());
        var scraper = new RecipeScraper(httpClient, Substitute.For<ILogger<RecipeScraper>>());

        // Act
        var result = await scraper.ScrapeAsync("https://127.0.0.1/recipe", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}
