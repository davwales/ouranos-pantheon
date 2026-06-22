using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Database;

public sealed class HestiaDbContext(DbContextOptions<HestiaDbContext> options)
    : OuranosDbContext(options, SchemaName)
{
    public const string SchemaName = "hestia";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
