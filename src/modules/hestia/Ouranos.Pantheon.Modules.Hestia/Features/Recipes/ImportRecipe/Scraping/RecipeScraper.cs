using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Security.AntiSSRF;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;

public sealed class RecipeScraper(HttpClient httpClient, ILogger<RecipeScraper> logger)
    : IRecipeScraper
{
    private const int MaxHtmlCharacters = 5_000_000;
    private const int ReadBufferSize = 16_384;

    private readonly HttpClient _httpClient = Guard.Against.Null(httpClient);
    private readonly ILogger<RecipeScraper> _logger = Guard.Against.Null(logger);

    public async Task<ScrapedJsonLdRecipe?> ScrapeAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        Guard.Against.NullOrWhiteSpace(url);
        _logger.LogTrace("Attempting to scrape recipe from '{url}'.", url);

        if (!IsValidHttpUrl(url))
        {
            _logger.LogWarning(
                "Recipe scrape of '{url}' skipped: url is not an absolute http(s) url.",
                url
            );
            return null;
        }

        HttpResponseMessage? response;
        try
        {
            response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
        }
        catch (AntiSSRFException exception)
        {
            _logger.LogWarning(
                exception,
                "Recipe scrape of '{url}' skipped: the url is not publicly routable.",
                url
            );
            return null;
        }

        using var page = response;

        response.EnsureSuccessStatusCode();

        if (!IsHtml(response))
        {
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "unknown";
            _logger.LogWarning(
                "Recipe scrape of '{url}' skipped: response is not HTML (content type '{contentType}').",
                url,
                contentType
            );
            return null;
        }

        var html = await ReadWithSizeLimitAsync(response, cancellationToken);
        if (html is null)
        {
            _logger.LogWarning(
                "Recipe scrape of '{url}' skipped: response exceeds the maximum allowed size.",
                url
            );
            return null;
        }

        var recipe = RecipeJsonLdParser.TryExtractRecipe(html);
        if (recipe is null)
        {
            _logger.LogWarning("Recipe scrape of '{url}' found no usable recipe metadata.", url);
        }
        else
        {
            _logger.LogDebug("Recipe scrape of '{url}' extracted '{title}'.", url, recipe.Title);
        }

        return recipe;
    }

    private static bool IsValidHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsHtml(HttpResponseMessage response)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;
        return contentType is not null
            && contentType.Contains("html", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> ReadWithSizeLimitAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var buffer = new char[ReadBufferSize];
        var builder = new StringBuilder();

        while (true)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(), cancellationToken);
            if (read == 0)
            {
                break;
            }

            builder.Append(buffer, 0, read);
            if (builder.Length > MaxHtmlCharacters)
            {
                return null;
            }
        }

        return builder.ToString();
    }
}
