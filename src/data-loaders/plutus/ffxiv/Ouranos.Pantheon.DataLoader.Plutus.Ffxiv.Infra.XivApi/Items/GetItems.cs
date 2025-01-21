using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Dtos;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Items;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Models;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Items;

public sealed class GetItems : IGetItems
{
    private const string CacheKey = $"XivAPi:{nameof(GetItems)}";
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private readonly IGithubClient _githubClient;
    private readonly ILogger<GetItems> _logger;

    public GetItems(
        IMemoryCache cache,
        ILogger<GetItems> logger,
        IGithubClient githubClient,
        IOptions<XivApiOptions> options
    )
    {
        Guard.Against.Null(cache);
        Guard.Against.Null(logger);
        Guard.Against.Null(githubClient);
        Guard.Against.Null(options);
        Guard.Against.Null(options.Value);

        _cache = cache;
        _logger = logger;
        _githubClient = githubClient;
        _cacheDuration = TimeSpan.FromMinutes(options.Value.ItemCacheMinutes);
    }

    public async Task<List<ItemDto>> GetItemsAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to get items from XivApi.");

        var xivItems = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return await _githubClient.GetItems(cancellationToken);
        }) ?? [];

        // We must add items for both lq and hq (if possible).
        var items = xivItems
            .Select(x => ConvertItem(x, false))
            .Union(xivItems
                .Where(x => x.CanBeHq)
                .Select(x => ConvertItem(x, true))
            )
            .ToList();

        _logger.LogDebug("Successfully retrieved items from XivApi.");
        return items;
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