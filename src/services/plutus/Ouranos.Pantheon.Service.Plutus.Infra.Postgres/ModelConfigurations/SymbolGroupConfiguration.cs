using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres.ModelConfigurations;

public sealed class SymbolGroupConfiguration : IEntityTypeConfiguration<SymbolGroup>
{
    public void Configure(EntityTypeBuilder<SymbolGroup> builder)
    {
        builder.HasKey(sg => sg.Id);
        builder.Property(sg => sg.Id).HasIdConversion();
        builder.Property(sg => sg.MarketId).HasIdConversion();
        builder.OwnsOne(sg => sg.SymbolIds).ToJson();
    }
}