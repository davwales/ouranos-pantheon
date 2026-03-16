using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database;

public sealed class HermesDesignTimeDbContextFactory : OuranosDesignTimeDbContextFactory<HermesDbContext>
{
    protected override HermesDbContext CreateDbContext(DbContextOptionsBuilder<HermesDbContext> optionsBuilder)
    {
        return new HermesDbContext(optionsBuilder.Options);
    }
}