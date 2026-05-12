using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv.XivApi;

public sealed class GithubClientTests
{
    private readonly ILogger<GithubClient> _logger = Substitute.For<ILogger<GithubClient>>();

    private static string BuildCsv(params (int key, string name, bool canBeHq)[] rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("junk1");
        sb.AppendLine("junk2");
        sb.AppendLine("junk3");
        sb.AppendLine(string.Join(",", Enumerable.Repeat("col", 89)));
        foreach (var (key, name, canBeHq) in rows)
        {
            var cols = Enumerable.Repeat("", 89).ToArray();
            cols[0] = key.ToString();
            cols[4] = name;
            cols[88] = canBeHq.ToString();
            sb.AppendLine(string.Join(",", cols));
        }
        return sb.ToString();
    }

    private GithubClient CreateClient(HttpStatusCode statusCode, string content)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://raw.githubusercontent.com/"),
        };
        return new GithubClient(_logger, httpClient);
    }

    [Fact]
    public async Task GetItems_WhenSuccessful_ShouldReturnParsedItems()
    {
        // Arrange
        var csv = BuildCsv((1, "Sword", true), (2, "Iron Ore", false));
        var client = CreateClient(HttpStatusCode.OK, csv);

        // Act
        var result = await client.GetItems(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetItems_WhenServerError_ShouldThrow()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.InternalServerError, "error");

        // Act
        var act = async () => await client.GetItems(CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetItems_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.OK, "");
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await client.GetItems(ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/plain"),
            };
            return Task.FromResult(response);
        }
    }
}
