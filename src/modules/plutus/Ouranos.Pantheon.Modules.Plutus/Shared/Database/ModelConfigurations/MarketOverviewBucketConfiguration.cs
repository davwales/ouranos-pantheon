using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class MarketOverviewBucketConfiguration
    : IEntityTypeConfiguration<MarketOverviewBucket>
{
    public void Configure(EntityTypeBuilder<MarketOverviewBucket> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasIdConversion();
        builder.Property(b => b.MarketId).HasIdConversion();
        builder.Property(b => b.TimeFrame).HasConversion<string>();
        builder.HasIndex(b => new { b.MarketId, b.TimeFrame });
        builder.HasOne(b => b.Market).WithMany().HasForeignKey(b => b.MarketId);
    }
}
