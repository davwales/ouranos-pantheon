using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.ModelConfigurations;

public sealed class MarketConfiguration : IEntityTypeConfiguration<Market>
{
    public void Configure(EntityTypeBuilder<Market> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasIdConversion();

        builder.OwnsOne(
            x => x.Taxes,
            taxes => taxes.OwnsOne(t => t.Flat)
        );
    }
}