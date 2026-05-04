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
            }
        );

        builder.Navigation(b => b.Results);
    }
}