using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres.ModelConfigurations;

public sealed class SymbolConfiguration : IEntityTypeConfiguration<Symbol>
{
    public void Configure(EntityTypeBuilder<Symbol> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();
        builder.OwnsOne(s => s.AdditionalFields);

        builder
            .HasIndex(s => new
                {
                    s.Code,
                    s.Subcode,
                    s.MarketId
                }
            )
            .IsUnique();
    }
}