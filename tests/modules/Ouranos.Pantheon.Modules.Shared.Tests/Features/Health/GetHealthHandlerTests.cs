using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Features.Health;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.Health;

public sealed class GetHealthHandlerTests
{
    private readonly ILogger<GetHealthHandler> _logger = Substitute.For<
        ILogger<GetHealthHandler>
    >();
    private readonly List<IHealthCheck> _checks = [];
    private readonly IOptions<HealthOptions> _options = Options.Create(new HealthOptions());

    private GetHealthHandler CreateHandler()
    {
        return new GetHealthHandler(_logger, _checks, _options);
    }

    [Fact]
    public async Task Handle_WhenAllChecksHealthy_ShouldReturnHealthy()
    {
        // Arrange
        var check1 = Substitute.For<IHealthCheck>();
        check1.Name.Returns("check1");
        check1
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(HealthStatus.Healthy, "OK", DateTime.UtcNow));

        var check2 = Substitute.For<IHealthCheck>();
        check2.Name.Returns("check2");
        check2
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(HealthStatus.Healthy, "OK", DateTime.UtcNow));

        _checks.Add(check1);
        _checks.Add(check2);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetHealthRequest(), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Checks["check1"].Status.ShouldBe(HealthStatus.Healthy);
        result.Checks["check2"].Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task Handle_WhenAnyCheckUnhealthy_ShouldReturnUnhealthy()
    {
        // Arrange
        var healthyCheck = Substitute.For<IHealthCheck>();
        healthyCheck.Name.Returns("healthy");
        healthyCheck
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(HealthStatus.Healthy, "OK", DateTime.UtcNow));

        var unhealthyCheck = Substitute.For<IHealthCheck>();
        unhealthyCheck.Name.Returns("unhealthy");
        unhealthyCheck
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(HealthStatus.Unhealthy, "Down", DateTime.UtcNow));

        _checks.Add(healthyCheck);
        _checks.Add(unhealthyCheck);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetHealthRequest(), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Checks["unhealthy"].Status.ShouldBe(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task Handle_WhenAnyCheckDegradedAndNoneUnhealthy_ShouldReturnDegraded()
    {
        // Arrange
        var healthyCheck = Substitute.For<IHealthCheck>();
        healthyCheck.Name.Returns("healthy");
        healthyCheck
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(HealthStatus.Healthy, "OK", DateTime.UtcNow));

        var degradedCheck = Substitute.For<IHealthCheck>();
        degradedCheck.Name.Returns("degraded");
        degradedCheck
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(HealthStatus.Degraded, "Slow", DateTime.UtcNow));

        _checks.Add(healthyCheck);
        _checks.Add(degradedCheck);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetHealthRequest(), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Checks["degraded"].Status.ShouldBe(HealthStatus.Degraded);
    }

    [Fact]
    public async Task Handle_WhenAllChecksNotConfigured_ShouldReturnNotConfigured()
    {
        // Arrange
        var check1 = Substitute.For<IHealthCheck>();
        check1.Name.Returns("check1");
        check1
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(
                new HealthCheckResult(HealthStatus.NotConfigured, "Not configured", DateTime.UtcNow)
            );

        var check2 = Substitute.For<IHealthCheck>();
        check2.Name.Returns("check2");
        check2
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(
                new HealthCheckResult(HealthStatus.NotConfigured, "Not configured", DateTime.UtcNow)
            );

        _checks.Add(check1);
        _checks.Add(check2);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetHealthRequest(), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.NotConfigured);
    }

    [Fact]
    public async Task Handle_WhenCheckTimesOut_ShouldReturnUnhealthyForThatCheck()
    {
        // Arrange
        var slowCheck = Substitute.For<IHealthCheck>();
        slowCheck.Name.Returns("slow");
        slowCheck
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var ct = call.Arg<CancellationToken>();
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                return new HealthCheckResult(HealthStatus.Healthy, "OK", DateTime.UtcNow);
            });

        _checks.Add(slowCheck);

        var shortTimeoutOptions = Options.Create(new HealthOptions { PerCheckTimeoutSeconds = 1 });
        var handler = new GetHealthHandler(_logger, _checks, shortTimeoutOptions);

        // Act
        var result = await handler.Handle(new GetHealthRequest(), CancellationToken.None);

        // Assert
        result.Checks["slow"].Status.ShouldBe(HealthStatus.Unhealthy);
        result.Checks["slow"].Description.ShouldContain("Timed out");
    }

    [Fact]
    public async Task Handle_WhenCheckThrowsException_ShouldReturnUnhealthyForThatCheck()
    {
        // Arrange
        var brokenCheck = Substitute.For<IHealthCheck>();
        brokenCheck.Name.Returns("broken");
        brokenCheck
            .CheckAsync(Arg.Any<CancellationToken>())
            .Returns<Task<HealthCheckResult>>(_ => throw new Exception("Something went wrong"));

        _checks.Add(brokenCheck);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetHealthRequest(), CancellationToken.None);

        // Assert
        result.Checks["broken"].Status.ShouldBe(HealthStatus.Unhealthy);
        result.Checks["broken"].Description.ShouldBe("Something went wrong");
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var handler = CreateHandler();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await handler.Handle(new GetHealthRequest(), cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenNoChecksRegistered_ShouldReturnHealthy()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetHealthRequest(), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Checks.ShouldBeEmpty();
    }
}
