using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class TraitConfiguration : IEntityTypeConfiguration<Trait>
{
    public void Configure(EntityTypeBuilder<Trait> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasIdConversion();
    }
}
