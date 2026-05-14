using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class AvailableModelConfiguration : IEntityTypeConfiguration<AvailableModel>
{
    public void Configure(EntityTypeBuilder<AvailableModel> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasIdConversion();
        builder.HasIndex(m => m.ModelIdentifier).IsUnique();
    }
}
