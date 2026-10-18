using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class ToolSetToolConfiguration : IEntityTypeConfiguration<ToolSetTool>
{
    public void Configure(EntityTypeBuilder<ToolSetTool> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasIdConversion();
        builder.Property(t => t.ToolSetId).HasIdConversion();
        builder.Property(t => t.Source).HasConversion<string>();
        builder
            .Property(t => t.AdditionalDescription)
            .HasMaxLength(ToolSetTool.MaxAdditionalDescriptionLength);

        builder
            .HasIndex(t => new
            {
                t.ToolSetId,
                t.Source,
                t.Name,
            })
            .IsUnique();
    }
}
