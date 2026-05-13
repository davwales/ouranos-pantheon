using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Domain.Notifications;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Notifications;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Postgres.Notifications;

public sealed class NotificationSenderJobTests
{
    private readonly ILogger<NotificationSenderJob> _logger = Substitute.For<
        ILogger<NotificationSenderJob>
    >();
    private readonly IDbContextFactory<SharedDbContext> _dbContextFactory;
    private readonly INotificationSender _sender = Substitute.For<INotificationSender>();
    private readonly TickerFunctionContext _tickerFunctionContext;
    private readonly NotificationSenderJob _job;
    private readonly string _dbName = Guid.NewGuid().ToString();

    public NotificationSenderJobTests()
    {
        _dbContextFactory = DbContextExtensions.MockFactory<SharedDbContext>(_dbName);
        _tickerFunctionContext = new TickerFunctionContext();
        _sender.Channel.Returns(NotificationChannel.Discord);

        _job = new NotificationSenderJob(
            _logger,
            _dbContextFactory,
            [_sender],
            Options.Create(new NotificationOptions())
        );
    }

    [Fact]
    public async Task Execute_WhenNoPendingNotifications_ShouldNotCallSender()
    {
        // Arrange

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _sender
            .DidNotReceive()
            .SendAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Execute_WhenPendingNotification_ShouldSendAndMarkSent()
    {
        // Arrange
        await using var seedContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );
        await seedContext.SeedData(notification);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _sender
            .Received(1)
            .SendAsync("channel-1", "Test Subject", "Test Message", Arg.Any<CancellationToken>());

        await using var verifyContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);
        var saved = verifyContext.Notifications.Single();
        saved.Status.ShouldBe(NotificationStatus.Sent);
        saved.SentAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Execute_WhenSenderNotRegistered_ShouldMarkFailed()
    {
        // Arrange
        await using var seedContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);
        var notification = Notification.Create(
            NotificationChannel.Email,
            "user@example.com",
            "Test Subject",
            "Test Message"
        );
        await seedContext.SeedData(notification);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _sender
            .DidNotReceive()
            .SendAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );

        await using var verifyContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);
        var saved = verifyContext.Notifications.Single();
        saved.Status.ShouldBe(NotificationStatus.Failed);
        saved.LastError.ShouldNotBeNullOrWhiteSpace();
        saved.LastError.ShouldContain("Email");
    }

    [Fact]
    public async Task Execute_WhenSenderThrows_ShouldMarkFailed()
    {
        // Arrange
        await using var seedContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );
        await seedContext.SeedData(notification);

        _sender
            .SendAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromException(new InvalidOperationException("Network error")));

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _sender
            .Received(1)
            .SendAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );

        await using var verifyContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);
        var saved = verifyContext.Notifications.Single();
        saved.Status.ShouldBe(NotificationStatus.Failed);
        saved.RetryCount.ShouldBe(1);
        saved.LastError.ShouldBe("Network error");
    }

    [Fact]
    public async Task Execute_WhenRetryCountExceedsMaxRetries_ShouldSkipNotification()
    {
        // Arrange
        var options = new NotificationOptions { MaxRetries = 3 };
        await using var seedContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);

        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );
        notification.MarkFailed("Error 1");
        notification.MarkFailed("Error 2");
        notification.MarkFailed("Error 3");
        notification.RetryCount.ShouldBe(3);

        await seedContext.SeedData(notification);

        var job = new NotificationSenderJob(
            _logger,
            _dbContextFactory,
            [_sender],
            Options.Create(options)
        );

        // Act
        await job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _sender
            .DidNotReceive()
            .SendAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );

        await using var verifyContext = DbContextExtensions.Mock<SharedDbContext>(_dbName);
        var saved = verifyContext.Notifications.Single();
        saved.Status.ShouldBe(NotificationStatus.Failed);
        saved.RetryCount.ShouldBe(3);
    }
}
