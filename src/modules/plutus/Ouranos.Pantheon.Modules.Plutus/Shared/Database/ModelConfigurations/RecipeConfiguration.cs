using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.ModelConfigurations;

public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasIdConversion();
        builder.Property(r => r.MarketId).HasIdConversion();

        builder.OwnsMany(r => r.Inputs, c => c.Property(i => i.SymbolId).HasIdConversion());
        builder.OwnsMany(r => r.Outputs, c => c.Property(i => i.SymbolId).HasIdConversion());
    }
}