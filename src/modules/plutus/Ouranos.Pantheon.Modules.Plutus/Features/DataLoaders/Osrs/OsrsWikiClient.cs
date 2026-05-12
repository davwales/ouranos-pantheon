using System.Net.Http.Json;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

public sealed class OsrsWikiClient : IWikiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OsrsWikiClient> _logger;

    public OsrsWikiClient(
        HttpClient httpClient,
        ILogger<OsrsWikiClient> logger,
        IOptions<OsrsDataLoaderOptions> options
    )
    {
        Guard.Against.Null(httpClient);
        Guard.Against.Null(logger);
        Guard.Against.Null(options);

        _httpClient = httpClient;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("User-Agent", options.Value.Wiki.UserAgent);
    }

    public async Task<List<Mapping>> GetMappings(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to get runescape mappings.");
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await _httpClient.GetAsync("mapping", cancellationToken);
        response.EnsureSuccessStatusCode();

        var mappings =
            await response.Content.ReadFromJsonAsync<List<Mapping>>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to read runescape mappings.");

        _logger.LogDebug(
            "Successfully retrieved {mappingCount} runescape mappings.",
            mappings.Count
        );
        return mappings;
    }

    public async Task<PriceResponse> GetPrices(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to get runescape prices.");
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await _httpClient.GetAsync("5m", cancellationToken);
        response.EnsureSuccessStatusCode();

        var prices =
            await response.Content.ReadFromJsonAsync<PriceResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to read runescape prices.");

        _logger.LogDebug(
            "Successfully retrieved {mappingCount} runescape prices with timestamp {timestamp}.",
            prices.Data.Count,
            prices.Timestamp
        );
        return prices;
    }
}
