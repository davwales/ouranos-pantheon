using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasIdConversion();
    }
}
