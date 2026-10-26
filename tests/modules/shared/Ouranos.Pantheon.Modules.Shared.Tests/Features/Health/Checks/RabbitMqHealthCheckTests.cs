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

    [Fact]
    public async Task CheckAsync_WhenConnectionFailsTwice_ShouldReturnUnhealthyEachTime()
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
        await using var check = new RabbitMqHealthCheck(options, _logger);
        await check.CheckAsync(CancellationToken.None);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task DisposeAsync_WhenNeverConnected_ShouldNotThrow()
    {
        // Arrange
        var check = new RabbitMqHealthCheck(Options.Create(new RabbitMqOptions()), _logger);

        // Act & Assert
        await Should.NotThrowAsync(async () => await check.DisposeAsync());
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrow()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new RabbitMqHealthCheck(null!, _logger));
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrow()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new RabbitMqHealthCheck(Options.Create(new RabbitMqOptions()), null!)
        );
    }
}
