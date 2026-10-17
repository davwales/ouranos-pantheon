using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class ImageMessagePartConfiguration : IEntityTypeConfiguration<ImageMessagePart>
{
    public void Configure(EntityTypeBuilder<ImageMessagePart> builder)
    {
        builder.Property(p => p.ImageId).HasIdConversion();
        builder.HasIndex(p => p.ImageId);
    }
}
