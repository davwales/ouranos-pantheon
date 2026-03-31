using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class SymbolGroupConfiguration : IEntityTypeConfiguration<SymbolGroup>
{
    public void Configure(EntityTypeBuilder<SymbolGroup> builder)
    {
        builder.HasKey(sg => sg.Id);
        builder.Property(sg => sg.Id).HasIdConversion();
        builder.Property(sg => sg.MarketId).HasIdConversion();

        builder.HasOne(sg => sg.Market).WithMany().HasForeignKey(sg => sg.MarketId);

        builder.HasMany(sg => sg.Members)
            .WithOne()
            .HasForeignKey(m => m.SymbolGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
