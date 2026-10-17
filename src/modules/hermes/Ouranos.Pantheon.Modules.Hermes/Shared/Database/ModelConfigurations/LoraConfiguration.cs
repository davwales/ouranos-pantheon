using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class LoraConfiguration : IEntityTypeConfiguration<Lora>
{
    public void Configure(EntityTypeBuilder<Lora> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasIdConversion();
        builder.HasIndex(l => l.Identifier).IsUnique();
    }
}
