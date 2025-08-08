using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Infra.Postgres;

public sealed class HermesDbContext(DbContextOptions<HermesDbContext> options) : OuranosDbContext(options, "hermes")
{
    public DbSet<Assistant> Assistants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Assistant>();
    }
}