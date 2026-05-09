using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Converters;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class PositionEntityConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasIdConversion();
        builder.Property(p => p.MarketId).HasIdConversion();
        builder.Property(p => p.SymbolId).HasIdConversion();
        builder.Property(p => p.LinkedBuyPositionId).HasConversion<IdConverter<Position>>();
        builder.Property(p => p.StrategyId).HasConversion<IdConverter<Strategy>>();

        builder.Property(p => p.Side).HasConversion<int>();
        builder.Property(p => p.Status).HasConversion<int>();

        builder.Property(p => p.Cost).HasPrecision(18, 2);
        builder.Property(p => p.Quantity).HasPrecision(18, 4);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.HasIndex(p => p.MarketId);
        builder.HasIndex(p => new { p.MarketId, p.Status });
        builder.HasIndex(p => p.SymbolId);

        builder.HasOne(p => p.Symbol)
            .WithMany()
            .HasForeignKey(p => p.SymbolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.LinkedBuyPosition)
            .WithMany()
            .HasForeignKey(p => p.LinkedBuyPositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
