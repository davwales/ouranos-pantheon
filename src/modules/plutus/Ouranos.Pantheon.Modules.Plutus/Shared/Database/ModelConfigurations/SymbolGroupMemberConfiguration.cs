using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class SymbolGroupMemberConfiguration : IEntityTypeConfiguration<SymbolGroupMember>
{
    public void Configure(EntityTypeBuilder<SymbolGroupMember> builder)
    {
        builder.HasKey(m => new { m.SymbolGroupId, m.SymbolId });
        builder.Property(m => m.SymbolGroupId).HasIdConversion();
        builder.Property(m => m.SymbolId).HasIdConversion();

        builder
            .HasOne(m => m.Symbol)
            .WithMany()
            .HasForeignKey(m => m.SymbolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
