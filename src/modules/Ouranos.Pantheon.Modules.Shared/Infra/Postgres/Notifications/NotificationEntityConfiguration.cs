using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Domain.Notifications;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Notifications;

public sealed class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasIdConversion();
        builder.Property(n => n.Channel).HasConversion<string>();
        builder.Property(n => n.Recipient).IsRequired();
        builder.Property(n => n.Subject).IsRequired();
        builder.Property(n => n.Message).IsRequired();
        builder.Property(n => n.Status).HasConversion<int>();
        builder.Property(n => n.RetryCount);
        builder.Property(n => n.SentAt);
        builder.Property(n => n.LastError);

        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => new { n.Channel, n.Status });
    }
}
