using System.Text.RegularExpressions;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

public static class ShoppingListNormalizer
{
    private static readonly Regex WhitespaceRun = new(@"\s+", RegexOptions.Compiled);

    public static string Normalize(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return trimmed;
        }

        return WhitespaceRun.Replace(trimmed, " ").ToLowerInvariant();
    }

    /// <summary>
    /// Identifies an ingredient regardless of plural ("eggs" and "egg"), so lines merge and keep their
    /// checked state and category when a recipe's wording changes.
    /// </summary>
    public static string NameKey(string name)
    {
        var normalized = Normalize(name);
        var lastSpace = normalized.LastIndexOf(' ');
        return normalized[..(lastSpace + 1)] + Singular(normalized[(lastSpace + 1)..]);
    }

    public static string RecipeLineKey(string nameKey, string measureKey)
    {
        return $"recipe:{nameKey}|{measureKey}";
    }

    public static string ManualLineKey(Guid manualItemId)
    {
        return $"manual:{manualItemId}";
    }

    // Suffix rules only: a wrong singular just yields a key that still matches itself.
    private static string Singular(string word)
    {
        if (word.Length <= 3 || word.EndsWith("ss", StringComparison.Ordinal))
        {
            return word;
        }

        if (word.EndsWith("ies", StringComparison.Ordinal))
        {
            return word[..^3] + "y";
        }

        if (word.EndsWith("oes", StringComparison.Ordinal))
        {
            return word[..^2];
        }

        return word.EndsWith('s') ? word[..^1] : word;
    }
}
