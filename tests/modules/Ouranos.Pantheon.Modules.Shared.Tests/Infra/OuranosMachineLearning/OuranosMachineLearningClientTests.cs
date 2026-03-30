using System.ClientModel;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAI;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningClientTests
{
    private readonly OuranosMachineLearningClient _client;
    private readonly HttpMessageHandler _httpMessageHandler;

    public OuranosMachineLearningClientTests()
    {
        _httpMessageHandler = Substitute.For<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandler) { BaseAddress = new Uri("http://test.com/") };

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential("test"),
            new OpenAIClientOptions { Endpoint = new Uri("http://test.com/") }
        );

        _client = new OuranosMachineLearningClient(
            Substitute.For<ILogger<OuranosMachineLearningClient>>(),
            httpClient,
            openAIClient
        );
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
            .Returns(Task.FromResult(new HttpResponseMessage { StatusCode = statusCode, Content = content }));
    }
}
