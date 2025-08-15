using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Models;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.ModelConfigurations;

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