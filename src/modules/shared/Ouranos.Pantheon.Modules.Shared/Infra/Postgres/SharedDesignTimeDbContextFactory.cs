using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;

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
