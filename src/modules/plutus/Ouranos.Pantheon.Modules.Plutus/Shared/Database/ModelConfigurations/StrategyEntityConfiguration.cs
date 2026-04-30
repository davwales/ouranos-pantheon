using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class StrategyEntityConfiguration : IEntityTypeConfiguration<Strategy>
{
    public void Configure(EntityTypeBuilder<Strategy> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();
        builder.Property(s => s.Type).HasConversion<int>();

        builder.HasOne(s => s.Market).WithMany().HasForeignKey(s => s.MarketId);
        builder.HasIndex(s => s.MarketId);

        builder.OwnsOne(s => s.Configuration, cfg =>
        {
            cfg.Property(c => c.BuyThreshold).HasPrecision(18, 2);
            cfg.Property(c => c.SellThreshold).HasPrecision(18, 2);
            cfg.Property(c => c.ForecastMovementThreshold).HasPrecision(18, 2);
            cfg.Property(c => c.ForecastHorizonDays);
            cfg.Property(c => c.DeviationMultiplier).HasPrecision(18, 2);
            cfg.Property(c => c.MeanTimeFrameValue);
            cfg.Property(c => c.MinMarginPercent).HasPrecision(18, 2);
            cfg.Property(c => c.MaxPositions);
            cfg.Property(c => c.MaxPositionPercent).HasPrecision(18, 2);
            cfg.Property(c => c.HoldPeriodDays);

            cfg.OwnsMany(c => c.SignalWeights, sw =>
            {
                sw.Property(w => w.Type).HasConversion<int>();
                sw.Property(w => w.Weight).HasPrecision(18, 2);
            });

            cfg.OwnsMany(c => c.Components, comp =>
            {
                comp.Property(c => c.StrategyId).HasIdConversion();
                comp.Property(c => c.Type).HasConversion<int>();
                comp.Property(c => c.Weight).HasPrecision(18, 2);
            });

            cfg.Navigation(c => c.SignalWeights);
            cfg.Navigation(c => c.Components);
        });

        builder.Navigation(s => s.Configuration).IsRequired();
    }
}