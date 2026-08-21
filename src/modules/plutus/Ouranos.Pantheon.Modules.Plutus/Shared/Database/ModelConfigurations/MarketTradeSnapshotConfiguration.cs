using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class MarketTradeSnapshotConfiguration : IEntityTypeConfiguration<MarketTradeSnapshot>
{
    public void Configure(EntityTypeBuilder<MarketTradeSnapshot> builder)
    {
        builder.HasKey(s => new { s.SymbolId, s.TimeFrame });
        builder.ToView("market_trade_snapshots");
        builder.Property(s => s.SymbolId).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();
        builder.Property(s => s.TimeFrame).HasConversion<string>();
    }
}
