using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Models;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Parsers;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi;

public sealed class GithubClient : IGithubClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GithubClient> _logger;

    public GithubClient(
        ILogger<GithubClient> logger,
        HttpClient httpClient
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(httpClient);

        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<List<ItemResponse>> GetItems(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to get items from the XivApi GitHub.");
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await _httpClient.GetAsync("ffxiv-datamining/master/csv/Item.csv", cancellationToken);
        response.EnsureSuccessStatusCode();

        var itemStream = await response.Content.ReadAsStreamAsync(cancellationToken)
                         ?? throw new InvalidOperationException("Failed to get item csv content.");
        var items = ItemParser.ParseItemCsv(itemStream);

        _logger.LogDebug("Successfully retrieved '{itemCount}' items from the XivApi GitHub.", items.Count);
        return items;
    }
}