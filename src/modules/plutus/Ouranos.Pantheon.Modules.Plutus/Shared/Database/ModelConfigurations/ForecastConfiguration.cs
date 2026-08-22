using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class ForecastConfiguration : IEntityTypeConfiguration<Forecast>
{
    public void Configure(EntityTypeBuilder<Forecast> builder)
    {
        builder.HasKey(p => new { p.Id, p.CreatedAt });

        builder.Property(p => p.Id).HasIdConversion();
        builder.Property(p => p.MarketId).HasIdConversion();
        builder.Property(p => p.SymbolId).HasIdConversion();

        builder.OwnsOne(
            p => p.Latest,
            x =>
            {
                x.Property(f => f.AveragePrice);
                x.Property(f => f.MinPrice);
                x.Property(f => f.MaxPrice);
                x.Property(f => f.Volume);
            }
        );
        builder.OwnsMany(p => p.Predictions, b => b.ToJson());
        builder.HasIndex(f => new { f.SymbolId, f.CreatedAt });
        builder.HasOne(f => f.Symbol).WithMany().HasForeignKey(f => f.SymbolId);
    }
}
