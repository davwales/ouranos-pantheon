using System.Text.Json;
using System.Xml;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;

/// <summary>
/// Reads yield, times and image from a schema.org Recipe node deterministically. Any value that
/// cannot be read becomes null; parsing never fails the import.
/// </summary>
internal static class RecipeJsonLdDetailsParser
{
    public static RecipeDetails Parse(JsonElement recipe)
    {
        return new RecipeDetails(
            ReadYield(recipe),
            ReadMinutes(recipe, "prepTime"),
            ReadMinutes(recipe, "cookTime"),
            ReadMinutes(recipe, "totalTime"),
            ReadImageUrl(recipe)
        );
    }

    private static string? ReadYield(JsonElement recipe)
    {
        if (!recipe.TryGetProperty("recipeYield", out var value))
        {
            return null;
        }

        IEnumerable<JsonElement> items =
            value.ValueKind == JsonValueKind.Array ? value.EnumerateArray() : [value];
        var candidates = items.Select(ReadScalar).OfType<string>().ToList();

        // Prefer a descriptive entry ("24 cookies") over a bare count ("24").
        var yield =
            candidates.FirstOrDefault(c => c.Any(ch => !char.IsDigit(ch) && !char.IsWhiteSpace(ch)))
            ?? candidates.FirstOrDefault();

        return yield is null ? null : Truncate(yield, RecipeDetails.MaxYieldLength);
    }

    private static int? ReadMinutes(JsonElement recipe, string property)
    {
        if (
            !recipe.TryGetProperty(property, out var value)
            || value.ValueKind != JsonValueKind.String
        )
        {
            return null;
        }

        try
        {
            var minutes = (int)
                Math.Round(XmlConvert.ToTimeSpan(value.GetString()!.Trim()).TotalMinutes);
            return minutes is > 0 and <= RecipeDetails.MaxMinutes ? minutes : null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static string? ReadImageUrl(JsonElement recipe)
    {
        return recipe.TryGetProperty("image", out var value) ? FindImageUrl(value) : null;
    }

    private static string? FindImageUrl(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                var url = value.GetString()?.Trim();
                return
                    url is { Length: > 0 and <= RecipeDetails.MaxImageUrlLength }
                    && RecipeDetails.IsHttpUrl(url)
                    ? url
                    : null;
            case JsonValueKind.Object:
                return value.TryGetProperty("url", out var objectUrl)
                    ? FindImageUrl(objectUrl)
                    : null;
            case JsonValueKind.Array:
                return value
                    .EnumerateArray()
                    .Select(FindImageUrl)
                    .FirstOrDefault(u => u is not null);
            default:
                return null;
        }
    }

    private static string? ReadScalar(JsonElement value)
    {
        var text = value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()?.Trim(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };

        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength].TrimEnd();
    }
}
