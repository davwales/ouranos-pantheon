using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class ForecastRecordConfiguration : IEntityTypeConfiguration<ForecastRecord>
{
    public void Configure(EntityTypeBuilder<ForecastRecord> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasIdConversion();
        builder.Property(r => r.RunId).HasIdConversion();
        builder.Property(r => r.MarketId).HasIdConversion();
        builder.Property(r => r.SymbolId).HasIdConversion();

        builder.OwnsOne(
            r => r.Predicted,
            x =>
            {
                x.Property(f => f.AveragePrice);
                x.Property(f => f.MinPrice);
                x.Property(f => f.MaxPrice);
                x.Property(f => f.Volume);
            }
        );

        builder.HasOne(r => r.Run).WithMany().HasForeignKey(r => r.RunId);
        builder.HasOne(r => r.Symbol).WithMany().HasForeignKey(r => r.SymbolId);

        builder.HasIndex(r => new { r.SymbolId, r.TargetAt });
        builder.HasIndex(r => new { r.SymbolId, r.GeneratedAt });
    }
}
