using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Infra.Postgres;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres;

public sealed class PlutusDesignTimeDbContextFactory : OuranosDesignTimeDbContextFactory<PlutusDbContext>
{
    protected override PlutusDbContext CreateDbContext(DbContextOptionsBuilder<PlutusDbContext> optionsBuilder)
    {
        return new PlutusDbContext(optionsBuilder.Options);
    }
}