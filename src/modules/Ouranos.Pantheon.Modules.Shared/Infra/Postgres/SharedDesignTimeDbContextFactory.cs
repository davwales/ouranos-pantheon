using Microsoft.EntityFrameworkCore;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

public sealed class SharedDesignTimeDbContextFactory
    : OuranosDesignTimeDbContextFactory<SharedDbContext>
{
    protected override SharedDbContext CreateDbContext(
        DbContextOptionsBuilder<SharedDbContext> optionsBuilder
    )
    {
        return new SharedDbContext(optionsBuilder.Options);
    }
}
