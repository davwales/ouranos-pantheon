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

    public static string RecipeLineKey(string normalizedName, string normalizedUnit)
    {
        return $"recipe:{normalizedName}|{normalizedUnit}";
    }

    public static string ManualLineKey(Guid manualItemId)
    {
        return $"manual:{manualItemId}";
    }
}
