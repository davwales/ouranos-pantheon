using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Tests;

public sealed class OuranosMachineLearningClientTests
{
    private readonly OuranosMachineLearningClient _client;
    private readonly HttpMessageHandler _httpMessageHandler;

    public OuranosMachineLearningClientTests()
    {
        _httpMessageHandler = Substitute.For<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandler)
        {
            BaseAddress = new Uri("http://test.com/")
        };

        _client = new OuranosMachineLearningClient(
            Substitute.For<ILogger<OuranosMachineLearningClient>>(),
            httpClient
        );
    }

    [Fact]
    public async Task GenerateCompletion_ShouldYieldExpectedChunks_WhenResponseIsSuccessful()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GenerateCompletionRequest>();
        var chunks = new[] { "chunk1", "chunk2", "chunk3" };
        SetupHttpHandler(HttpStatusCode.OK, new StreamContent(chunks.AsMemoryStream()));

        // Act
        var result = await _client.GenerateCompletion(request).ToListAsync();

        // Assert
        result.ShouldBe(["chunk1chunk2chunk3"]);
        _httpMessageHandler.Protected(
            "SendAsync",
            Arg.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString() == "http://test.com/generation/text"
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GenerateCompletion_WhenGivenNewLineChunk_ShouldIncludeNewLine()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GenerateCompletionRequest>();
        var chunks = new[] { "chunk1", "\n", "  ", "chunk2" };
        SetupHttpHandler(HttpStatusCode.OK, new StreamContent(chunks.AsMemoryStream()));

        // Act
        var result = await _client.GenerateCompletion(request).ToListAsync();

        // Assert
        result.ShouldBe(["chunk1\n  chunk2"]);
    }

    [Fact]
    public async Task GenerateCompletion_ShouldThrowException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GenerateCompletionRequest>();
        SetupHttpHandler(HttpStatusCode.BadRequest);

        // Act
        var generate = async () => await _client.GenerateCompletion(request).ToListAsync();

        // Assert
        await generate.ShouldThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateCompletion_ShouldRespectCancellation()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GenerateCompletionRequest>();
        var cancellationToken = new CancellationToken(true);
        var chunks = new[] { "some content" };
        SetupHttpHandler(HttpStatusCode.OK, new StreamContent(chunks.AsMemoryStream()));

        // Act
        var generate = async () =>
            await _client.GenerateCompletion(request, cancellationToken).ToListAsync(cancellationToken);

        // Assert
        await generate.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetPlutusForecasts_ShouldReturnExpectedResults()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GetPlutusForecastsRequest>();
        var expectedForecasts = fixture.Create<List<List<ForecastPoint>>>();
        var jsonBody = JsonSerializer.Serialize(expectedForecasts);
        SetupHttpHandler(HttpStatusCode.OK, new StringContent(jsonBody));

        // Act
        var actualForecasts = await _client.GetPlutusForecasts(request);

        // Assert
        actualForecasts.ShouldBe(expectedForecasts);
    }

    [Fact]
    public async Task GetPlutusForecasts_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GetPlutusForecastsRequest>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _client.GetPlutusForecasts(request, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetPlutusForecasts_WhenResponseNotSuccessful_ShouldThrowHttpRequestException()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GetPlutusForecastsRequest>();
        SetupHttpHandler(HttpStatusCode.BadRequest);

        // Act
        var get = async () => await _client.GetPlutusForecasts(request);

        // Assert
        await get.ShouldThrowAsync<HttpRequestException>();
    }

    private void SetupHttpHandler(HttpStatusCode statusCode, HttpContent? content = null)
    {
        _httpMessageHandler
            .Protected(
                "SendAsync",
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Task.FromResult(
                    new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = content
                    }
                )
            );
    }
}