using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Infra.Database;

public sealed class HermesDesignTimeDbContextFactory : OuranosDesignTimeDbContextFactory<HermesDbContext>
{
    protected override HermesDbContext CreateDbContext(DbContextOptionsBuilder<HermesDbContext> optionsBuilder)
    {
        return new HermesDbContext(optionsBuilder.Options);
    }
}