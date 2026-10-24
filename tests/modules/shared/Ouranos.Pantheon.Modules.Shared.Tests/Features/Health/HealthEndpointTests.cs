using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Shared.Features.Health;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.Health;

public sealed class HealthEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Theory]
    [InlineData(HealthStatus.Healthy, StatusCodes.Status200OK)]
    [InlineData(HealthStatus.Degraded, StatusCodes.Status200OK)]
    [InlineData(HealthStatus.NotConfigured, StatusCodes.Status200OK)]
    [InlineData(HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable)]
    public async Task Handle_WhenStatusIs_ShouldReturnMatchingStatusCode(
        HealthStatus status,
        int expectedStatusCode
    )
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new GetHealthResponse(status, []);

        _bus.InvokeAsync<GetHealthResponse>(Arg.Any<GetHealthRequest>(), ct)
            .Returns(Task.FromResult(expected));

        // Act
        var result = await HealthEndpoint.Handle(_bus, ct);

        // Assert
        var json = result.ShouldBeOfType<JsonHttpResult<GetHealthResponse>>();
        json.StatusCode.ShouldBe(expectedStatusCode);
        json.Value.ShouldBe(expected);
    }
}
