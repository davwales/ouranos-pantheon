using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Enums;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.Health.Checks;

public sealed class TickerQHealthCheckTests
{
    private readonly ILogger<TickerQHealthCheck> _logger = Substitute.For<
        ILogger<TickerQHealthCheck>
    >();
    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();

    [Fact]
    public async Task CheckAsync_WhenNoEnabledTickers_ShouldReturnNotConfigured()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContextFactory = DbContextExtensions.MockFactory<TickerQDbContext>(dbName);
        SetupScopeFactory(dbContextFactory);

        var check = new TickerQHealthCheck(_scopeFactory, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.NotConfigured);
        result.Description.ShouldContain("No enabled tickers found");
    }

    [Fact]
    public async Task CheckAsync_WhenAllTickersHealthy_ShouldReturnHealthy()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContextFactory = DbContextExtensions.MockFactory<TickerQDbContext>(dbName);

        await using var seedContext = DbContextExtensions.Mock<TickerQDbContext>(dbName);
        var ticker = new CronTickerEntity
        {
            Id = Guid.NewGuid(),
            IsEnabled = true,
            Description = "test-ticker",
            Expression = "* * * * *",
            Function = "TestFunction",
        };
        var occurrence = new CronTickerOccurrenceEntity<CronTickerEntity>
        {
            Id = Guid.NewGuid(),
            CronTickerId = ticker.Id,
            Status = TickerStatus.Done,
            ExceptionMessage = null,
            ExecutionTime = DateTime.UtcNow,
        };
        seedContext.Set<CronTickerEntity>().Add(ticker);
        seedContext.Set<CronTickerOccurrenceEntity<CronTickerEntity>>().Add(occurrence);
        await seedContext.SaveChangesAsync();

        SetupScopeFactory(dbContextFactory);

        var check = new TickerQHealthCheck(_scopeFactory, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckAsync_WhenAnyTickerHasException_ShouldReturnUnhealthy()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContextFactory = DbContextExtensions.MockFactory<TickerQDbContext>(dbName);

        await using var seedContext = DbContextExtensions.Mock<TickerQDbContext>(dbName);
        var ticker = new CronTickerEntity
        {
            Id = Guid.NewGuid(),
            IsEnabled = true,
            Description = "failing-ticker",
            Expression = "* * * * *",
            Function = "FailFunction",
        };
        var occurrence = new CronTickerOccurrenceEntity<CronTickerEntity>
        {
            Id = Guid.NewGuid(),
            CronTickerId = ticker.Id,
            Status = TickerStatus.Done,
            ExceptionMessage = "Execution failed",
            ExecutionTime = DateTime.UtcNow,
        };
        seedContext.Set<CronTickerEntity>().Add(ticker);
        seedContext.Set<CronTickerOccurrenceEntity<CronTickerEntity>>().Add(occurrence);
        await seedContext.SaveChangesAsync();

        SetupScopeFactory(dbContextFactory);

        var check = new TickerQHealthCheck(_scopeFactory, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Description.ShouldContain("failed");
    }

    [Fact]
    public async Task CheckAsync_WhenAnyTickerNeverRan_ShouldReturnDegraded()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContextFactory = DbContextExtensions.MockFactory<TickerQDbContext>(dbName);

        await using var seedContext = DbContextExtensions.Mock<TickerQDbContext>(dbName);
        var ticker = new CronTickerEntity
        {
            Id = Guid.NewGuid(),
            IsEnabled = true,
            Description = "never-ran-ticker",
            Expression = "* * * * *",
            Function = "NeverRanFunction",
        };
        seedContext.Set<CronTickerEntity>().Add(ticker);
        await seedContext.SaveChangesAsync();

        SetupScopeFactory(dbContextFactory);

        var check = new TickerQHealthCheck(_scopeFactory, _logger);

        // Act
        var result = await check.CheckAsync(CancellationToken.None);

        // Assert
        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldContain("never ran");
    }

    private void SetupScopeFactory(IDbContextFactory<TickerQDbContext> dbContextFactory)
    {
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        _scopeFactory.CreateAsyncScope().Returns(new AsyncServiceScope(scope));
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider
            .GetService(typeof(IDbContextFactory<TickerQDbContext>))
            .Returns(dbContextFactory);
    }
}
