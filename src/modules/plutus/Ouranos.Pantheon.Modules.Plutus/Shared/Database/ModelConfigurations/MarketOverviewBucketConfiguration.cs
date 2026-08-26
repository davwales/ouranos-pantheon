using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class MarketOverviewBucketConfiguration
    : IEntityTypeConfiguration<MarketOverviewBucket>
{
    public void Configure(EntityTypeBuilder<MarketOverviewBucket> builder)
    {
        builder.HasKey(b => new
        {
            b.MarketId,
            b.TimeFrame,
            b.BucketStart,
        });
        builder.ToView("market_overview_buckets");
        builder.Property(b => b.MarketId).HasIdConversion();
        builder.Property(b => b.TimeFrame).HasConversion<string>();
    }
}
