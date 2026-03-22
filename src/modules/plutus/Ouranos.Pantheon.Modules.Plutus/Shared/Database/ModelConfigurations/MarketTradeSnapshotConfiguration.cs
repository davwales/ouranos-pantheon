using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class MarketTradeSnapshotConfiguration : IEntityTypeConfiguration<MarketTradeSnapshot>
{
    public void Configure(EntityTypeBuilder<MarketTradeSnapshot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();
        builder.Property(s => s.SymbolId).HasIdConversion();
        builder.Property(s => s.TimeFrame).HasConversion<string>();
        builder.HasIndex(s => new { s.SymbolId, s.TimeFrame }).IsUnique();
        builder.HasIndex(s => new { s.MarketId, s.TimeFrame });
        builder.HasOne(s => s.Symbol).WithMany().HasForeignKey(s => s.SymbolId);
        builder.HasOne(s => s.Market).WithMany().HasForeignKey(s => s.MarketId);
    }
}
