using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres.ModelConfigurations;

public sealed class ForecastConfiguration : IEntityTypeConfiguration<Forecast>
{
    public void Configure(EntityTypeBuilder<Forecast> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasIdConversion();
        builder.Property(p => p.MarketId).HasIdConversion();
        builder.Property(p => p.SymbolId).HasIdConversion();

        builder.OwnsOne(p => p.Latest);
        builder.OwnsMany(p => p.Predictions);
    }
}