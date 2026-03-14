using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Infra.Database;

public sealed class HermesDbContext(DbContextOptions<HermesDbContext> options) : OuranosDbContext(options, "hermes")
{
    public DbSet<Assistant> Assistants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Assistant>();
    }
}