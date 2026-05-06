using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

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
        builder.Property(b => b.ProgressPercent).HasDefaultValue(0);
        builder.Property(b => b.ProgressMessage).HasMaxLength(256);
        builder.Property(b => b.Budget).HasPrecision(18, 2);

        builder.HasOne(b => b.Strategy).WithMany().HasForeignKey(b => b.StrategyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(b => b.StrategyId);
        builder.HasIndex(b => b.Status);

        builder.OwnsOne(
            b => b.Results,
            r =>
            {
                r.Property(res => res.TotalReturn).HasPrecision(18, 2);
                r.Property(res => res.TotalReturnPercent).HasPrecision(18, 2);
                r.Property(res => res.MaxDrawdown).HasPrecision(18, 2);
                r.Property(res => res.MaxDrawdownPercent).HasPrecision(18, 2);
                r.Property(res => res.WinRate).HasPrecision(18, 2);
                r.Property(res => res.TotalTrades);
                r.Property(res => res.WinningTrades);
                r.Property(res => res.LosingTrades);
                r.Property(res => res.SharpeRatio).HasPrecision(18, 2);
                r.Property(res => res.AverageTradeReturn).HasPrecision(18, 2);
                r.Property(res => res.BestTrade).HasPrecision(18, 2);
                r.Property(res => res.WorstTrade).HasPrecision(18, 2);
                r.Property(res => res.FinalBalance).HasPrecision(18, 2);

                r.OwnsMany(
                    res => res.Positions,
                    p =>
                    {
                        p.Property(pos => pos.EntryPrice).HasPrecision(18, 2);
                        p.Property(pos => pos.ExitPrice).HasPrecision(18, 2);
                        p.Property(pos => pos.Volume).HasPrecision(18, 2);
                        p.Property(pos => pos.ProfitLoss).HasPrecision(18, 2);
                        p.Property(pos => pos.ReturnPercent).HasPrecision(18, 2);
                    }
                );

                r.Navigation(res => res.Positions);

                r.OwnsOne(
                    res => res.OptimizedConfiguration,
                    c =>
                    {
                        c.Property(sc => sc.BuyThreshold).HasPrecision(18, 2);
                        c.Property(sc => sc.SellThreshold).HasPrecision(18, 2);
                        c.Property(sc => sc.ForecastMovementThreshold).HasPrecision(18, 2);
                        c.Property(sc => sc.ForecastHorizonDays);
                        c.Property(sc => sc.DeviationMultiplier).HasPrecision(18, 2);
                        c.Property(sc => sc.MeanTimeFrameValue);
                        c.Property(sc => sc.MinMarginPercent).HasPrecision(18, 2);
                        c.Property(sc => sc.MaxPositions);
                        c.Property(sc => sc.MaxPositionPercent).HasPrecision(18, 2);
                        c.Property(sc => sc.HoldPeriodDays);

                        c.OwnsMany(
                            sc => sc.SignalWeights,
                            sw =>
                            {
                                sw.ToTable("backtest_optimized_signal_weights", "plutus");
                                sw.Property(w => w.Weight).HasPrecision(18, 2);
                                sw.Property(w => w.Type).HasConversion<int>();
                            }
                        );

                        c.OwnsMany(
                            sc => sc.Components,
                            cc =>
                            {
                                cc.ToTable("backtest_optimized_components", "plutus");
                                cc.Property(comp => comp.StrategyId).HasIdConversion();
                                cc.Property(comp => comp.Weight).HasPrecision(18, 2);
                                cc.Property(comp => comp.Type).HasConversion<int>();
                            }
                        );

                        c.Navigation(sc => sc.SignalWeights);
                        c.Navigation(sc => sc.Components);
                    }
                );
            }
        );

        builder.Navigation(b => b.Results);
    }
}