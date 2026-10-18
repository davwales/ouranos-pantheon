using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class ModelConfigConfiguration : IEntityTypeConfiguration<ModelConfig>
{
    public void Configure(EntityTypeBuilder<ModelConfig> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasIdConversion();
        builder.Property(m => m.DefaultToolSetId).HasConversion<IdConverter<ToolSet>>();

        builder
            .HasOne<ToolSet>()
            .WithMany()
            .HasForeignKey(m => m.DefaultToolSetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
