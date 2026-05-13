using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class BacktestEntityConfiguration : IEntityTypeConfiguration<Backtest>
{
    public void Configure(EntityTypeBuilder<Backtest> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasIdConversion();
        builder.Property(b => b.StrategyId).HasIdConversion();
        builder.Property(b => b.MarketId).HasIdConversion();
        builder.Property(b => b.Status).HasConversion<int>();
        builder
            .Property(b => b.Kind)
            .HasConversion<string>()
            .HasDefaultValue(BacktestKind.Backtest);
        builder.Property(b => b.ProgressPercent).HasDefaultValue(0);
        builder.Property(b => b.ProgressMessage).HasMaxLength(256);
        builder.Property(b => b.Budget).HasPrecision(18, 2);

        builder.OwnsOne(
            b => b.Results,
            results =>
            {
                results.Property(r => r.TotalReturn).HasPrecision(18, 2);
                results.Property(r => r.TotalReturnPercent).HasPrecision(18, 2);
                results.Property(r => r.MaxDrawdown).HasPrecision(18, 2);
                results.Property(r => r.MaxDrawdownPercent).HasPrecision(18, 2);
                results.Property(r => r.WinRate).HasPrecision(18, 2);
                results.Property(r => r.SharpeRatio).HasPrecision(18, 2);
                results.Property(r => r.SortinoRatio).HasPrecision(18, 2);
                results.Property(r => r.CalmarRatio).HasPrecision(18, 2);
                results.Property(r => r.Cagr).HasPrecision(18, 2);
                results.Property(r => r.ProfitFactor).HasPrecision(18, 2);
                results.Property(r => r.Expectancy).HasPrecision(18, 2);
                results.Property(r => r.AverageTradeReturn).HasPrecision(18, 2);
                results.Property(r => r.BestTrade).HasPrecision(18, 2);
                results.Property(r => r.WorstTrade).HasPrecision(18, 2);
                results.Property(r => r.FinalBalance).HasPrecision(18, 2);

                results.OwnsOne(
                    r => r.OptimizedSignalWeightedConfig,
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

                results.OwnsOne(
                    r => r.OptimizedForecastMomentumConfig,
                    config =>
                    {
                        config.Property(c => c.ForecastMovementThreshold).HasPrecision(18, 2);
                        config.Property(c => c.ForecastHorizonDays);
                    }
                );

                results.OwnsOne(
                    r => r.OptimizedMeanReversionConfig,
                    config =>
                    {
                        config.Property(c => c.DeviationMultiplier).HasPrecision(18, 2);
                        config.Property(c => c.MeanTimeFrameValue);
                    }
                );

                results.OwnsOne(
                    r => r.OptimizedRecipeArbitrageConfig,
                    config =>
                    {
                        config.Property(c => c.MinMarginPercent).HasPrecision(18, 2);
                    }
                );

                results.OwnsOne(
                    r => r.OptimizedConfiguration,
                    config =>
                    {
                        config.Property(c => c.MaxPositions);
                        config.Property(c => c.MaxPositionPercent).HasPrecision(18, 2);
                        config.Property(c => c.HoldPeriodDays);
                    }
                );
            }
        );

        builder
            .HasOne(b => b.Strategy)
            .WithMany()
            .HasForeignKey(b => b.StrategyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(b => b.StrategyId);
        builder.HasIndex(b => b.Status);

        builder.OwnsMany(
            b => b.Positions,
            p =>
            {
                p.Property(pos => pos.EntryPrice).HasPrecision(18, 2);
                p.Property(pos => pos.ExitPrice).HasPrecision(18, 2);
                p.Property(pos => pos.Volume).HasPrecision(18, 2);
                p.Property(pos => pos.ProfitLoss).HasPrecision(18, 2);
                p.Property(pos => pos.ReturnPercent).HasPrecision(18, 2);
            }
        );

        builder.Navigation(b => b.Positions);
    }
}
