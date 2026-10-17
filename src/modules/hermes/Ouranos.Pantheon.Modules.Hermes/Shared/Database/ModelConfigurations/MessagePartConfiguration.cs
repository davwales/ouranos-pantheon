using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class MessagePartConfiguration : IEntityTypeConfiguration<MessagePart>
{
    public void Configure(EntityTypeBuilder<MessagePart> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasIdConversion();
        builder.Property(p => p.MessageId).HasIdConversion();

        builder.HasIndex(p => new { p.MessageId, p.SortOrder });

        builder
            .HasDiscriminator<string>("kind")
            .HasValue<ReasoningMessagePart>("reasoning")
            .HasValue<TextMessagePart>("text")
            .HasValue<ToolCallMessagePart>("tool_call");
    }
}
