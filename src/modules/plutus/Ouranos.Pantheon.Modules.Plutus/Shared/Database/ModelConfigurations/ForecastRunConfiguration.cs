using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class ForecastRunConfiguration : IEntityTypeConfiguration<ForecastRun>
{
    public void Configure(EntityTypeBuilder<ForecastRun> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasIdConversion();
        builder.Property(r => r.ModelName).IsRequired();

        builder.HasIndex(r => r.GeneratedAt);
    }
}