using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class StrategyEntityConfiguration : IEntityTypeConfiguration<Strategy>
{
    public void Configure(EntityTypeBuilder<Strategy> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();

        builder.HasOne(s => s.Market).WithMany().HasForeignKey(s => s.MarketId);
        builder.HasIndex(s => s.MarketId);

        builder.OwnsOne(
            s => s.TradingConfiguration,
            config =>
            {
                config.Property(c => c.MaxPositions);
                config.Property(c => c.MaxPositionPercent).HasPrecision(18, 2);
                config.Property(c => c.HoldPeriodDays);
            }
        );

        builder.OwnsOne(
            s => s.Thresholds,
            config =>
            {
                config.Property(t => t.BuyThreshold).HasPrecision(18, 2);
                config.Property(t => t.SellThreshold).HasPrecision(18, 2);
            }
        );

        builder.OwnsMany(
            s => s.InputWeights,
            w =>
            {
                w.Property(x => x.Kind).HasConversion<int>();
                w.Property(x => x.Weight).HasPrecision(18, 2);
                w.HasIndex("StrategyId", "Kind").IsUnique();
            }
        );

        builder.Navigation(s => s.InputWeights);
    }
}
