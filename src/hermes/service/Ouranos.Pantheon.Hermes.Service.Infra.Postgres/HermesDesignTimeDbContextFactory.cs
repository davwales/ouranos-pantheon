using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Infra.Postgres;

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres;

public sealed class HermesDesignTimeDbContextFactory : OuranosDesignTimeDbContextFactory<HermesDbContext>
{
    protected override HermesDbContext CreateDbContext(DbContextOptionsBuilder<HermesDbContext> optionsBuilder)
    {
        return new HermesDbContext(optionsBuilder.Options);
    }
}