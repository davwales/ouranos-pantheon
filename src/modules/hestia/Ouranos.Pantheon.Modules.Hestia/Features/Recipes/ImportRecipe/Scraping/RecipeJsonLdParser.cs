using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;

internal static partial class RecipeJsonLdParser
{
    [GeneratedRegex(
        """<script\b[^>]*\btype\s*=\s*("|')application/ld\+json\1[^>]*>(.*?)</script>""",
        RegexOptions.IgnoreCase | RegexOptions.Singleline
    )]
    private static partial Regex JsonLdScriptRegex();

    public static ScrapedJsonLdRecipe? TryExtractRecipe(string html)
    {
        foreach (Match match in JsonLdScriptRegex().Matches(html))
        {
            var recipe = TryExtractFromJson(match.Groups[2].Value);
            if (recipe is not null)
            {
                return recipe;
            }
        }

        return null;
    }

    private static ScrapedJsonLdRecipe? TryExtractFromJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(
                json.TrimStart('\uFEFF'),
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip,
                }
            );
            return FindRecipe(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ScrapedJsonLdRecipe? FindRecipe(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var candidate in element.EnumerateArray())
            {
                var recipe = TryReadRecipe(candidate);
                if (recipe is not null)
                {
                    return recipe;
                }
            }

            return null;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var rootRecipe = TryReadRecipe(element);
        if (rootRecipe is not null)
        {
            return rootRecipe;
        }

        if (
            element.TryGetProperty("@graph", out var graph)
            && graph.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var child in graph.EnumerateArray())
            {
                var recipe = FindRecipe(child);
                if (recipe is not null)
                {
                    return recipe;
                }
            }
        }

        return null;
    }

    private static ScrapedJsonLdRecipe? TryReadRecipe(JsonElement element)
    {
        if (
            element.ValueKind != JsonValueKind.Object
            || !MatchesRecipeType(element)
            || !TryGetTitle(element, out var title)
            || !HasIngredients(element)
            || !HasInstructions(element)
        )
        {
            return null;
        }

        return new ScrapedJsonLdRecipe(title, element.GetRawText());
    }

    private static bool MatchesRecipeType(JsonElement node)
    {
        if (!node.TryGetProperty("@type", out var type))
        {
            return false;
        }

        if (type.ValueKind == JsonValueKind.String)
        {
            return string.Equals(type.GetString(), "Recipe", StringComparison.OrdinalIgnoreCase);
        }

        if (type.ValueKind == JsonValueKind.Array)
        {
            foreach (var candidate in type.EnumerateArray())
            {
                if (
                    candidate.ValueKind == JsonValueKind.String
                    && string.Equals(
                        candidate.GetString(),
                        "Recipe",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryGetTitle(JsonElement node, out string title)
    {
        title = string.Empty;

        if (!node.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        title = name.GetString()?.Trim() ?? string.Empty;
        return title.Length > 0;
    }

    private static bool HasIngredients(JsonElement node)
    {
        return HasStringOrStringArray(node, "recipeIngredient")
            || HasStringOrStringArray(node, "ingredients");
    }

    private static bool HasStringOrStringArray(JsonElement node, string property)
    {
        if (!node.TryGetProperty(property, out var value))
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return true;
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasInstructions(JsonElement node)
    {
        if (!node.TryGetProperty("recipeInstructions", out var value))
        {
            return false;
        }

        switch (value.ValueKind)
        {
            case JsonValueKind.String:
            case JsonValueKind.Object:
                return true;
            case JsonValueKind.Array:
                foreach (var item in value.EnumerateArray())
                {
                    if (item.ValueKind is JsonValueKind.String or JsonValueKind.Object)
                    {
                        return true;
                    }
                }

                return false;
            default:
                return false;
        }
    }
}
