using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Ouranos.Pantheon.Modules.Shared.Infra.RabbitMq;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.Health.Checks;

public sealed class RabbitMqHealthCheckTests
{
    private readonly ILogger<RabbitMqHealthCheck> _logger = Substitute.For<
        ILogger<RabbitMqHealthCheck>
    >();

    [Fact]
    public async Task CheckAsync_WhenHostIsEmpty_ShouldReturnNotConfigured()
    {
        // Arrange
        var options = Options.Create(new RabbitMqOptions());
        var check = new RabbitMqHealthCheck(options, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.NotConfigured);
        result.Description.ShouldContain("not configured");
    }

    [Fact]
    public async Task CheckAsync_WhenConnectionFails_ShouldReturnUnhealthy()
    {
        // Arrange
        var options = Options.Create(
            new RabbitMqOptions(
                Host: "invalid-host-that-does-not-exist",
                Username: "guest",
                Password: "guest",
                RetryCount: null
            )
        );
        var check = new RabbitMqHealthCheck(options, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }
}
