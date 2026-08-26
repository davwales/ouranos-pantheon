using System.Text.Json;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

internal static class StructuredCompletionParser
{
    private static readonly JsonSerializerOptions ParseOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
    };

    public static T? Parse<T>(string? content)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var json = Unwrap(content);

        try
        {
            return JsonSerializer.Deserialize<T>(json, ParseOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Unwrap(string content)
    {
        var trimmed = content.Trim();

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewline > 0 && lastFence > firstNewline)
            {
                return trimmed[(firstNewline + 1)..lastFence].Trim();
            }
        }

        return trimmed;
    }
}
