using System.Net;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Tests;

public sealed class OuranosMachineLearningClientTests
{
    private readonly OuranosMachineLearningClient _client;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<ILogger<OuranosMachineLearningClient>> _loggerMock = new();

    public OuranosMachineLearningClientTests()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://test.com/")
        };

        _client = new OuranosMachineLearningClient(_loggerMock.Object, httpClient);
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

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString() == "http://test.com/generation/text"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GenerateCompletion_ShouldSkipEmptyChunks()
    {
        // Arrange
        var fixture = new Fixture();
        var request = fixture.Create<GenerateCompletionRequest>();
        var chunks = new[] { "chunk1", "", "  ", "chunk2" };
        SetupHttpHandler(HttpStatusCode.OK, new StreamContent(chunks.AsMemoryStream()));

        // Act
        var result = await _client.GenerateCompletion(request).ToListAsync();

        // Assert
        result.ShouldBe(["chunk1  chunk2"]);
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

    private void SetupHttpHandler(HttpStatusCode statusCode, HttpContent? content = null)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content
            });
    }
}