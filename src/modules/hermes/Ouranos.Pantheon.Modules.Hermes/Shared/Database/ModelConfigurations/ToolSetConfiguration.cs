using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class ToolSetConfiguration : IEntityTypeConfiguration<ToolSet>
{
    public void Configure(EntityTypeBuilder<ToolSet> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.Instructions).HasMaxLength(ToolSet.MaxInstructionsLength);

        builder
            .HasMany(s => s.Tools)
            .WithOne()
            .HasForeignKey(t => t.ToolSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
