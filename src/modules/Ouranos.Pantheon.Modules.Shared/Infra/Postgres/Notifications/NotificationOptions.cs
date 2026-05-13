namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Notifications;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    public int MaxRetries { get; init; } = 3;
}
