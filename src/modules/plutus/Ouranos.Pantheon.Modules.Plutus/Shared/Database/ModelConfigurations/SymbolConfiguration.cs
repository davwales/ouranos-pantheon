using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class SymbolConfiguration : IEntityTypeConfiguration<Symbol>
{
    public void Configure(EntityTypeBuilder<Symbol> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();
        builder.OwnsOne(s => s.AdditionalFields);
        builder.HasOne(s => s.Market).WithMany().HasForeignKey(s => s.MarketId);

        builder
            .HasIndex(s => new
            {
                s.Code,
                s.Subcode,
                s.MarketId,
            })
            .IsUnique();
    }
}
