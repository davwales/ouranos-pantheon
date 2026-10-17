using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class ToolCallMessagePartConfiguration : IEntityTypeConfiguration<ToolCallMessagePart>
{
    public void Configure(EntityTypeBuilder<ToolCallMessagePart> builder)
    {
        builder.Property(p => p.Status).HasConversion<string>();
    }
}
