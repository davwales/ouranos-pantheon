using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.Health.Checks;

public sealed class OuranosMlHealthCheckTests
{
    private readonly ILogger<OuranosMlHealthCheck> _logger = Substitute.For<
        ILogger<OuranosMlHealthCheck>
    >();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();

    [Fact]
    public async Task CheckAsync_WhenConnectionStringIsEmpty_ShouldReturnNotConfigured()
    {
        // Arrange
        var options = Options.Create(new OuranosMachineLearningOptions());
        var check = new OuranosMlHealthCheck(options, _httpClientFactory, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.NotConfigured);
        result.Description.ShouldContain("not configured");
    }

    [Fact]
    public async Task CheckAsync_WhenHealthEndpointReturns200_ShouldReturnHealthy()
    {
        // Arrange
        var options = Options.Create(
            new OuranosMachineLearningOptions(
                ConnectionString: "http://localhost",
                ApiKey: "test-key"
            )
        );

        var handler = new StubHttpMessageHandler(HttpStatusCode.OK);
        var client = new HttpClient(handler);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(client);

        var check = new OuranosMlHealthCheck(options, _httpClientFactory, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckAsync_WhenHealthEndpointReturnsNon200_ShouldReturnUnhealthy()
    {
        // Arrange
        var options = Options.Create(
            new OuranosMachineLearningOptions(
                ConnectionString: "http://localhost",
                ApiKey: "test-key"
            )
        );

        var handler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError);
        var client = new HttpClient(handler);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(client);

        var check = new OuranosMlHealthCheck(options, _httpClientFactory, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode = statusCode;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            return Task.FromResult(
                new HttpResponseMessage(_statusCode) { ReasonPhrase = _statusCode.ToString() }
            );
        }
    }
}
