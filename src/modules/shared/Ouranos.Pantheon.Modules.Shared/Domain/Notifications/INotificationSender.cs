namespace Ouranos.Pantheon.Modules.Shared.Domain.Notifications;

public interface INotificationSender
{
    NotificationChannel Channel { get; }

    Task SendAsync(
        string recipient,
        string subject,
        string message,
        CancellationToken cancellationToken = default
    );
}
