using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class SignalConfiguration : IEntityTypeConfiguration<Signal>
{
    public void Configure(EntityTypeBuilder<Signal> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();
        builder.Property(s => s.SymbolId).HasIdConversion();
        builder.Property(s => s.Type).HasConversion<int>();
        builder.HasIndex(s => new { s.SymbolId, s.Type });
        builder.HasIndex(s => s.ComputedAt).IsDescending(true);
        builder.HasIndex(s => new { s.SymbolId, s.ComputedAt }).IsDescending(false, true);
        builder.HasIndex(s => s.MarketId);
        builder.HasOne(s => s.Market).WithMany().HasForeignKey(s => s.MarketId);
        builder.HasOne(s => s.Symbol).WithMany().HasForeignKey(s => s.SymbolId);
    }
}
