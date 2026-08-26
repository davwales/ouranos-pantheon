using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.Health.Checks;

public sealed class PostgresHealthCheckTests
{
    private readonly ILogger<PostgresHealthCheck> _logger = Substitute.For<
        ILogger<PostgresHealthCheck>
    >();

    [Fact]
    public async Task CheckAsync_WhenHostIsEmpty_ShouldReturnNotConfigured()
    {
        // Arrange
        var options = Options.Create(new PostgresOptions());
        var check = new PostgresHealthCheck(options, _logger);

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
            new PostgresOptions(
                Host: "invalid-host-that-does-not-exist",
                Port: 5432,
                Database: "nonexistent",
                Username: "invalid",
                Password: "invalid",
                SearchPath: null,
                IncludeErrorDetail: false,
                CommandTimeout: 1,
                MaxRetries: 0,
                MaxRetryDelaySeconds: 1,
                EnableSensitiveDataLogging: false
            )
        );
        var check = new PostgresHealthCheck(options, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }
}
