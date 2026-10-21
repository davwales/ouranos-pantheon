using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

public sealed record RecipeDetails
{
    public const int MaxYieldLength = 100;
    public const int MaxMinutes = 10_080;
    public const int MaxImageUrlLength = 2_000;

    public static readonly RecipeDetails Empty = new(null, null, null, null, null);

    public string? Yield { get; init; }
    public int? PrepMinutes { get; init; }
    public int? CookMinutes { get; init; }
    public int? TotalMinutes { get; init; }
    public string? ImageUrl { get; init; }

    public RecipeDetails(
        string? yield,
        int? prepMinutes,
        int? cookMinutes,
        int? totalMinutes,
        string? imageUrl
    )
    {
        var trimmedYield = string.IsNullOrWhiteSpace(yield) ? null : yield.Trim();
        if (trimmedYield is not null)
        {
            Guard.Against.OutOfRange(trimmedYield.Length, nameof(yield), 1, MaxYieldLength);
        }

        GuardMinutes(prepMinutes, nameof(prepMinutes));
        GuardMinutes(cookMinutes, nameof(cookMinutes));
        GuardMinutes(totalMinutes, nameof(totalMinutes));

        var trimmedImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        if (trimmedImageUrl is not null)
        {
            Guard.Against.OutOfRange(
                trimmedImageUrl.Length,
                nameof(imageUrl),
                1,
                MaxImageUrlLength
            );
            Guard.Against.InvalidInput(
                trimmedImageUrl,
                nameof(imageUrl),
                IsHttpUrl,
                "Image url must be an absolute http(s) URL."
            );
        }

        Yield = trimmedYield;
        PrepMinutes = prepMinutes;
        CookMinutes = cookMinutes;
        TotalMinutes = totalMinutes;
        ImageUrl = trimmedImageUrl;
    }

    public static bool IsHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static void GuardMinutes(int? minutes, string parameterName)
    {
        if (minutes is not null)
        {
            Guard.Against.OutOfRange(minutes.Value, parameterName, 0, MaxMinutes);
        }
    }
}
