using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Ouranos.Pantheon.Modules.Shared.WebSockets;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.Health.Checks;

public sealed class WebSocketHealthCheckTests
{
    private readonly ILogger<WebSocketHealthCheck> _logger = Substitute.For<
        ILogger<WebSocketHealthCheck>
    >();

    [Fact]
    public async Task CheckAsync_WhenNoWorkersRegistered_ShouldReturnNotConfigured()
    {
        // Arrange
        var healthState = new WebSocketHealthState();
        var check = new WebSocketHealthCheck(healthState, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.NotConfigured);
        result.Description.ShouldContain("No WebSocket workers registered");
    }

    [Fact]
    public async Task CheckAsync_WhenAllWorkersConnected_ShouldReturnHealthy()
    {
        // Arrange
        var healthState = new WebSocketHealthState();
        healthState.Report("worker1", true);
        healthState.Report("worker2", true);

        var check = new WebSocketHealthCheck(healthState, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Description.ShouldContain("worker1");
        result.Description.ShouldContain("worker2");
    }

    [Fact]
    public async Task CheckAsync_WhenSomeWorkersDisconnected_ShouldReturnDegraded()
    {
        // Arrange
        var healthState = new WebSocketHealthState();
        healthState.Report("worker1", true);
        healthState.Report("worker2", false);

        var check = new WebSocketHealthCheck(healthState, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldContain("worker1");
        result.Description.ShouldContain("worker2");
    }

    [Fact]
    public async Task CheckAsync_WhenAllWorkersDisconnected_ShouldReturnUnhealthy()
    {
        // Arrange
        var healthState = new WebSocketHealthState();
        healthState.Report("worker1", false);
        healthState.Report("worker2", false);

        var check = new WebSocketHealthCheck(healthState, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Description.ShouldContain("All WebSocket connections are down");
    }
}
