using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;
using Ouranos.Pantheon.Modules.Shared.Domain.Notifications;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

public sealed class SharedDbContext(DbContextOptions<SharedDbContext> options)
    : OuranosDbContext(options, "shared")
{
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>();
    }
}
