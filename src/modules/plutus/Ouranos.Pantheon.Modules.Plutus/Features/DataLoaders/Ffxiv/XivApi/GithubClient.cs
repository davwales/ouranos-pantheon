using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public sealed class GithubClient : IGithubClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GithubClient> _logger;

    public GithubClient(ILogger<GithubClient> logger, HttpClient httpClient)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(httpClient);

        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<List<ItemResponse>> GetItems(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to get items from the XivApi GitHub.");
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await _httpClient.GetAsync(
            "ffxiv-datamining/refs/heads/master/csv/en/Item.csv",
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var itemStream =
            await response.Content.ReadAsStreamAsync(cancellationToken)
            ?? throw new InvalidOperationException("Failed to get item csv content.");
        var items = ItemParser.ParseItemCsv(itemStream);

        _logger.LogDebug(
            "Successfully retrieved '{itemCount}' items from the XivApi GitHub.",
            items.Count
        );
        return items;
    }
}
