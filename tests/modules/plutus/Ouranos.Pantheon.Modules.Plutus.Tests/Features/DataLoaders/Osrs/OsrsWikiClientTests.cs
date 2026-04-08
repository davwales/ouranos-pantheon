using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Osrs;

public sealed class OsrsWikiClientTests
{
    private readonly ILogger<OsrsWikiClient> _logger = Substitute.For<ILogger<OsrsWikiClient>>();

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string content)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://prices.runescape.wiki/api/v1/osrs/") };
        return client;
    }

    private OsrsWikiClient CreateClient(HttpClient httpClient) =>
        new(httpClient, _logger, Options.Create(new OsrsDataLoaderOptions(
            IsEnabled: true,
            RefreshIntervalMinutes: 5,
            Wiki: new OsrsWikiOptions("https://prices.runescape.wiki/api/v1/osrs/", "TestAgent/1.0")
        )));

    [Fact]
    public async Task GetMappings_WhenSuccessful_ShouldReturnMappings()
    {
        // Arrange
        var mappings = new[]
        {
            new { id = 1234, name = "Sword", icon = "sword.png", examine = "A sword", members = true, lowalch = 100, highalch = 200, limit = 50, value = 500 }
        };
        var json = JsonSerializer.Serialize(mappings, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var client = CreateClient(CreateHttpClient(HttpStatusCode.OK, json));

        // Act
        var result = await client.GetMappings(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(1234);
        result[0].Name.ShouldBe("Sword");
    }

    [Fact]
    public async Task GetPrices_WhenSuccessful_ShouldReturnPrices()
    {
        // Arrange
        var priceResponse = new
        {
            data = new Dictionary<string, object>
            {
                ["1234"] = new { avgHighPrice = 500, highPriceVolume = 10, avgLowPrice = 450, lowPriceVolume = 8 }
            },
            timestamp = 1700000000
        };
        var json = JsonSerializer.Serialize(priceResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var client = CreateClient(CreateHttpClient(HttpStatusCode.OK, json));

        // Act
        var result = await client.GetPrices(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Timestamp.ShouldBe(1700000000);
        result.Data.ContainsKey("1234").ShouldBeTrue();
    }

    [Fact]
    public async Task GetMappings_WhenServerError_ShouldThrow()
    {
        // Arrange
        var client = CreateClient(CreateHttpClient(HttpStatusCode.InternalServerError, "error"));

        // Act
        var act = async () => await client.GetMappings(CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetMappings_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var client = CreateClient(CreateHttpClient(HttpStatusCode.OK, "[]"));
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await client.GetMappings(ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
