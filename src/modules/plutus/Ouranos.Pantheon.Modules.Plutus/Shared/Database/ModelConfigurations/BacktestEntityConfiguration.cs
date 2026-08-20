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
        builder.Property(b => b.UpdatedAt).IsConcurrencyToken();
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
                results.Property(r => r.TurnoverRate).HasPrecision(18, 2);

                results.OwnsOne(
                    r => r.OptimizedThresholds,
                    config =>
                    {
                        config.Property(t => t.BuyThreshold).HasPrecision(18, 2);
                        config.Property(t => t.SellThreshold).HasPrecision(18, 2);
                    }
                );

                results.OwnsMany(
                    r => r.OptimizedInputWeights,
                    w =>
                    {
                        w.Property(x => x.Kind).HasConversion<int>();
                        w.Property(x => x.Weight).HasPrecision(18, 2);
                        w.HasIndex("BacktestResultsBacktestId", "Kind").IsUnique();
                    }
                );

                results.Navigation(r => r.OptimizedInputWeights);

                results.OwnsOne(
                    r => r.OptimizedConfiguration,
                    config =>
                    {
                        config.Property(c => c.MaxPositions);
                        config.Property(c => c.MaxPositionPercent).HasPrecision(18, 2);
                        config.Property(c => c.HoldPeriodDays);
                    }
                );

                results.OwnsOne(
                    r => r.OutSampleResults,
                    oos =>
                    {
                        oos.Property(o => o.TotalReturn).HasPrecision(18, 2);
                        oos.Property(o => o.TotalReturnPercent).HasPrecision(18, 2);
                        oos.Property(o => o.MaxDrawdown).HasPrecision(18, 2);
                        oos.Property(o => o.MaxDrawdownPercent).HasPrecision(18, 2);
                        oos.Property(o => o.WinRate).HasPrecision(18, 2);
                        oos.Property(o => o.SharpeRatio).HasPrecision(18, 2);
                        oos.Property(o => o.SortinoRatio).HasPrecision(18, 2);
                        oos.Property(o => o.CalmarRatio).HasPrecision(18, 2);
                        oos.Property(o => o.Cagr).HasPrecision(18, 2);
                        oos.Property(o => o.ProfitFactor).HasPrecision(18, 2);
                        oos.Property(o => o.Expectancy).HasPrecision(18, 2);
                        oos.Property(o => o.AverageTradeReturn).HasPrecision(18, 2);
                        oos.Property(o => o.BestTrade).HasPrecision(18, 2);
                        oos.Property(o => o.WorstTrade).HasPrecision(18, 2);
                        oos.Property(o => o.FinalBalance).HasPrecision(18, 2);
                        oos.Ignore(o => o.OutSampleResults);
                        oos.Ignore(o => o.OptimizedInputWeights);
                        oos.Ignore(o => o.OptimizedThresholds);
                        oos.Ignore(o => o.OptimizedConfiguration);
                        oos.Ignore(o => o.IsValidated);
                        oos.Ignore(o => o.TurnoverRate);
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
