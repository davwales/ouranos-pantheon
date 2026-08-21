using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class ForecastRecordWithActualConfiguration
    : IEntityTypeConfiguration<ForecastRecordWithActual>
{
    public void Configure(EntityTypeBuilder<ForecastRecordWithActual> builder)
    {
        builder.HasKey(e => e.Id);
        builder.ToView("forecast_records_with_actuals");
        builder.Property(e => e.Id).HasIdConversion();
        builder.Property(e => e.RunId).HasIdConversion();
        builder.Property(e => e.MarketId).HasIdConversion();
        builder.Property(e => e.SymbolId).HasIdConversion();
    }
}
