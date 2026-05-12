using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database;

public sealed class PlutusDesignTimeDbContextFactory
    : OuranosDesignTimeDbContextFactory<PlutusDbContext>
{
    protected override PlutusDbContext CreateDbContext(
        DbContextOptionsBuilder<PlutusDbContext> optionsBuilder
    )
    {
        return new PlutusDbContext(optionsBuilder.Options);
    }
}
