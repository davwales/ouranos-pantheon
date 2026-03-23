using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database;

public sealed class HermesDbContext(DbContextOptions<HermesDbContext> options) : OuranosDbContext(options, "hermes")
{
    public DbSet<Persona> Personas { get; set; }

    public DbSet<ModelConfig> ModelConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Persona>();
        modelBuilder.Entity<ModelConfig>();
    }
}
