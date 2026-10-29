using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public sealed class GetItems : IGetItems
{
    private const string CacheKey = $"XivAPi:{nameof(GetItems)}";
    private readonly IMemoryCache _cache;
    private readonly IGithubClient _githubClient;
    private readonly ILogger<GetItems> _logger;
    private readonly IOptions<FfxivDataLoaderOptions> _options;

    public GetItems(
        IMemoryCache cache,
        ILogger<GetItems> logger,
        IGithubClient githubClient,
        IOptions<FfxivDataLoaderOptions> options
    )
    {
        Guard.Against.Null(cache);
        Guard.Against.Null(logger);
        Guard.Against.Null(githubClient);
        Guard.Against.Null(options);

        _cache = cache;
        _logger = logger;
        _githubClient = githubClient;
        _options = options;
    }

    public async Task<ItemDto?> GetItemAsync(
        string symbolCode,
        bool isHighQuality,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to get item '{symbolCode}' '{isHighQuality}' from XivApi.",
            symbolCode,
            isHighQuality
        );

        var items =
            await _cache.GetOrCreateAsync(
                CacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                        _options.Value.XivApi.ItemCacheMinutes
                    );
                    return BuildLookup(await _githubClient.GetItems(cancellationToken));
                }
            ) ?? [];

        return items.GetValueOrDefault((symbolCode, isHighQuality));
    }

    private static Dictionary<(string SymbolCode, bool IsHighQuality), ItemDto> BuildLookup(
        List<ItemResponse> xivItems
    )
    {
        // We must add items for both lq and hq (if possible). The first entry per key wins.
        var lookup = new Dictionary<(string SymbolCode, bool IsHighQuality), ItemDto>();
        var items = xivItems
            .Select(x => ConvertItem(x, false))
            .Concat(xivItems.Where(x => x.CanBeHq).Select(x => ConvertItem(x, true)));

        foreach (var item in items)
        {
            lookup.TryAdd((item.SymbolCode, item.IsHighQuality), item);
        }

        return lookup;
    }

    private static ItemDto ConvertItem(ItemResponse itemResponse, bool isHighQuality)
    {
        return new ItemDto(
            itemResponse.Key.ToString(),
            isHighQuality,
            itemResponse.Name,
            new AdditionalFields()
        );
    }
}
