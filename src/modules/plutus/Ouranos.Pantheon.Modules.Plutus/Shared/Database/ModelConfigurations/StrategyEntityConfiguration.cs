using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class StrategyEntityConfiguration : IEntityTypeConfiguration<Strategy>
{
    public void Configure(EntityTypeBuilder<Strategy> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasIdConversion();
        builder.Property(s => s.MarketId).HasIdConversion();

        builder.HasOne(s => s.Market).WithMany().HasForeignKey(s => s.MarketId);
        builder.HasIndex(s => s.MarketId);

        builder.Property(s => s.Configuration).HasJsonConversion();
    }
}
