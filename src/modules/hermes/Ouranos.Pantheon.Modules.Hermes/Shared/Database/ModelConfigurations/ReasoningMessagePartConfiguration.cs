using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class ReasoningMessagePartConfiguration
    : IEntityTypeConfiguration<ReasoningMessagePart>
{
    public void Configure(EntityTypeBuilder<ReasoningMessagePart> builder)
    {
        builder.Property(p => p.Content).HasColumnName("content");
    }
}
