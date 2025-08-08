using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Infra.Postgres.ModelConfigurations;

public sealed class AssistantConfiguration : IEntityTypeConfiguration<Assistant>
{
    public void Configure(EntityTypeBuilder<Assistant> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasIdConversion();
    }
}