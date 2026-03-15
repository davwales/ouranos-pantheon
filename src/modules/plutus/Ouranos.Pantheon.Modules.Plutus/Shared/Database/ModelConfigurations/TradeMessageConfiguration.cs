using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class TradeMessageConfiguration : IEntityTypeConfiguration<TradeMessage>
{
    public void Configure(EntityTypeBuilder<TradeMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.MessageId).IsUnique();

        builder.Property(x => x.Id).HasIdConversion();
        builder.Property(x => x.TradeId).HasIdConversion();
    }
}