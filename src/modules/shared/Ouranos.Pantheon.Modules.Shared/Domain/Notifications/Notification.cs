using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Shared.Domain.Notifications;

public sealed class Notification : BaseEntity<Id<Notification>>
{
    private Notification()
        : base(DatabaseExtensions.CreateId<Notification>()) { }

    public NotificationChannel Channel { get; private set; }

    public string Recipient { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public string? LastError { get; private set; }

    public static Notification Create(
        NotificationChannel channel,
        string recipient,
        string subject,
        string message
    )
    {
        Guard.Against.NullOrWhiteSpace(recipient);
        Guard.Against.NullOrWhiteSpace(subject);
        Guard.Against.NullOrWhiteSpace(message);

        var notification = new Notification
        {
            Channel = channel,
            Recipient = recipient,
            Subject = subject,
            Message = message,
            Status = NotificationStatus.Pending,
        };

        return notification;
    }

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        Update();
    }

    public void MarkFailed(string error)
    {
        Guard.Against.NullOrWhiteSpace(error);

        Status = NotificationStatus.Failed;
        RetryCount++;
        LastError = error;
        Update();
    }

    public void Retry()
    {
        Status = NotificationStatus.Pending;
        Update();
    }
}
