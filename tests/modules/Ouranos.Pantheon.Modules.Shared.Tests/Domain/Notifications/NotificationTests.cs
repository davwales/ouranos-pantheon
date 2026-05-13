using Ouranos.Pantheon.Modules.Shared.Domain.Notifications;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Domain.Notifications;

public sealed class NotificationTests
{
    [Fact]
    public void Create_WhenValidArgs_ShouldSetProperties()
    {
        // Arrange
        var channel = NotificationChannel.Discord;
        var recipient = "channel-1";
        var subject = "Test Subject";
        var message = "Test Message";

        // Act
        var notification = Notification.Create(channel, recipient, subject, message);

        // Assert
        notification.Channel.ShouldBe(channel);
        notification.Recipient.ShouldBe(recipient);
        notification.Subject.ShouldBe(subject);
        notification.Message.ShouldBe(message);
    }

    [Fact]
    public void Create_WhenValidArgs_ShouldSetStatusToPending()
    {
        // Arrange
        var channel = NotificationChannel.Discord;
        var recipient = "channel-1";
        var subject = "Test Subject";
        var message = "Test Message";

        // Act
        var notification = Notification.Create(channel, recipient, subject, message);

        // Assert
        notification.Status.ShouldBe(NotificationStatus.Pending);
    }

    [Fact]
    public void Create_WhenNullRecipient_ShouldThrow()
    {
        // Arrange
        string? recipient = null;
        var channel = NotificationChannel.Discord;
        var subject = "Test Subject";
        var message = "Test Message";

        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
            Notification.Create(channel, recipient!, subject, message)
        );

        // Assert
        exception.ParamName.ShouldBe("recipient");
    }

    [Fact]
    public void Create_WhenNullSubject_ShouldThrow()
    {
        // Arrange
        var channel = NotificationChannel.Discord;
        var recipient = "channel-1";
        string? subject = null;
        var message = "Test Message";

        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
            Notification.Create(channel, recipient, subject!, message)
        );

        // Assert
        exception.ParamName.ShouldBe("subject");
    }

    [Fact]
    public void Create_WhenNullMessage_ShouldThrow()
    {
        // Arrange
        var channel = NotificationChannel.Discord;
        var recipient = "channel-1";
        var subject = "Test Subject";
        string? message = null;

        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
            Notification.Create(channel, recipient, subject, message!)
        );

        // Assert
        exception.ParamName.ShouldBe("message");
    }

    [Fact]
    public void MarkSent_ShouldSetStatusToSent()
    {
        // Arrange
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );

        // Act
        notification.MarkSent();

        // Assert
        notification.Status.ShouldBe(NotificationStatus.Sent);
    }

    [Fact]
    public void MarkSent_ShouldSetSentAt()
    {
        // Arrange
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );
        var before = DateTimeOffset.UtcNow;

        // Act
        notification.MarkSent();

        // Assert
        notification.SentAt.ShouldNotBeNull();
        notification.SentAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        notification.SentAt.Value.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void MarkFailed_WhenValidError_ShouldSetStatusToFailed()
    {
        // Arrange
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );

        // Act
        notification.MarkFailed("Something went wrong");

        // Assert
        notification.Status.ShouldBe(NotificationStatus.Failed);
    }

    [Fact]
    public void MarkFailed_WhenValidError_ShouldIncrementRetryCount()
    {
        // Arrange
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );
        var initialRetryCount = notification.RetryCount;

        // Act
        notification.MarkFailed("Something went wrong");

        // Assert
        notification.RetryCount.ShouldBe(initialRetryCount + 1);
    }

    [Fact]
    public void MarkFailed_WhenValidError_ShouldSetLastError()
    {
        // Arrange
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );
        var error = "Something went wrong";

        // Act
        notification.MarkFailed(error);

        // Assert
        notification.LastError.ShouldBe(error);
    }

    [Fact]
    public void MarkFailed_WhenNullError_ShouldThrow()
    {
        // Arrange
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );

        // Act
        var exception = Should.Throw<ArgumentNullException>(() => notification.MarkFailed(null!));

        // Assert
        exception.ParamName.ShouldBe("error");
    }

    [Fact]
    public void Retry_ShouldSetStatusToPending()
    {
        // Arrange
        var notification = Notification.Create(
            NotificationChannel.Discord,
            "channel-1",
            "Test Subject",
            "Test Message"
        );
        notification.MarkFailed("Something went wrong");
        notification.Status.ShouldBe(NotificationStatus.Failed);

        // Act
        notification.Retry();

        // Assert
        notification.Status.ShouldBe(NotificationStatus.Pending);
    }
}
