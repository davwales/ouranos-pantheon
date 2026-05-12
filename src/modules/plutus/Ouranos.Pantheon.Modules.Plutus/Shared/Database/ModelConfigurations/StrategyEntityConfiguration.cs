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
            s => s.SignalWeightedConfig,
            config =>
            {
                config.Property(c => c.BuyThreshold).HasPrecision(18, 2);
                config.Property(c => c.SellThreshold).HasPrecision(18, 2);
                config.Property(c => c.TaxAdjustedRoiWeight).HasPrecision(18, 2);
                config.Property(c => c.VolumeAnomalyWeight).HasPrecision(18, 2);
                config.Property(c => c.TrendMomentumWeight).HasPrecision(18, 2);
                config.Property(c => c.BollingerBandsWeight).HasPrecision(18, 2);
                config.Property(c => c.RsiWeight).HasPrecision(18, 2);
                config.Property(c => c.MovingAverageCrossoverWeight).HasPrecision(18, 2);
                config.Property(c => c.PriceVelocityWeight).HasPrecision(18, 2);
            }
        );

        builder.OwnsOne(
            s => s.ForecastMomentumConfig,
            config =>
            {
                config.Property(c => c.ForecastMovementThreshold).HasPrecision(18, 2);
                config.Property(c => c.ForecastHorizonDays);
            }
        );

        builder.OwnsOne(
            s => s.MeanReversionConfig,
            config =>
            {
                config.Property(c => c.DeviationMultiplier).HasPrecision(18, 2);
                config.Property(c => c.MeanTimeFrameValue);
            }
        );

        builder.OwnsOne(
            s => s.RecipeArbitrageConfig,
            config => { config.Property(c => c.MinMarginPercent).HasPrecision(18, 2); }
        );

        builder.OwnsMany(
            s => s.Components,
            comp =>
            {
                comp.ToTable("composite_component", "plutus");
                comp.Property(c => c.StrategyId).HasIdConversion();
                comp.Property(c => c.Type).HasConversion<int>();
                comp.Property(c => c.Weight).HasPrecision(18, 2);
            }
        );

        builder.Navigation(s => s.Components);
    }
}