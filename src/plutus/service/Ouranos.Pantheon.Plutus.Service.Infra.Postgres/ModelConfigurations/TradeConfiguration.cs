using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.ModelConfigurations;

public sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.Property(t => t.Id).HasIdConversion();
        builder.Property(t => t.SymbolId).HasIdConversion();
    }
}