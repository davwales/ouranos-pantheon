using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.ForecastGenerator;

public sealed class ForecastModelClientTests
{
    private readonly ILogger<ForecastModelClient> _logger = Substitute.For<
        ILogger<ForecastModelClient>
    >();

    private static readonly ForecastModelRequest Request = new(
        2,
        [
            [new ForecastModelPoint(100m, 90m, 110m, 5m)],
        ]
    );

    private ForecastModelClient CreateClient(RecordingHandler handler)
    {
        return new ForecastModelClient(
            _logger,
            new HttpClient(handler) { BaseAddress = new Uri("http://ml.test/") }
        );
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new ForecastModelClient(null!, new HttpClient()));
    }

    [Fact]
    public void Constructor_WhenHttpClientIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new ForecastModelClient(_logger, null!));
    }

    [Fact]
    public async Task ForecastAsync_WhenSuccessful_ShouldPostSnakeCaseAndReturnThePredictions()
    {
        // Arrange
        var handler = new RecordingHandler(
            HttpStatusCode.OK,
            """[[{"average_price":101,"min_price":91,"max_price":111,"volume":6}]]"""
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.ForecastAsync(Request);

        // Assert
        handler.RequestUri.ShouldBe(new Uri("http://ml.test/plutus/forecast"));
        var body = handler.RequestBody.ShouldNotBeNull();
        body["num_predictions"]!.GetValue<int>().ShouldBe(2);
        body["points"]![0]![0]!["average_price"]!.GetValue<decimal>().ShouldBe(100m);
        result.ShouldHaveSingleItem().ShouldBe([new ForecastModelPoint(101m, 91m, 111m, 6m)]);
    }

    [Fact]
    public async Task ForecastAsync_WhenRequestFails_ShouldThrow()
    {
        // Arrange
        var client = CreateClient(new RecordingHandler(HttpStatusCode.BadGateway, "{}"));

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() => client.ForecastAsync(Request));
    }

    private sealed class RecordingHandler(HttpStatusCode statusCode, string content)
        : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public JsonNode? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestUri = request.RequestUri;
            if (request.Content is not null)
            {
                RequestBody = JsonNode.Parse(
                    await request.Content.ReadAsStringAsync(cancellationToken)
                );
            }

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
            };
        }
    }
}
