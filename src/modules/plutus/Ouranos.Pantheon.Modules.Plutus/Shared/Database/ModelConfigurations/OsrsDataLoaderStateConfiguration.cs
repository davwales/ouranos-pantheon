using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.DataLoaders;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class OsrsDataLoaderStateConfiguration : IEntityTypeConfiguration<OsrsDataLoaderState>
{
    public void Configure(EntityTypeBuilder<OsrsDataLoaderState> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasData(new OsrsDataLoaderState { Id = OsrsDataLoaderState.SingletonId });
    }
}
