using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database;

public sealed class HermesDbContext(DbContextOptions<HermesDbContext> options)
    : OuranosDbContext(options, "hermes")
{
    public DbSet<Persona> Personas { get; set; }

    public DbSet<ModelConfig> ModelConfigs { get; set; }

    public DbSet<AvailableModel> AvailableModels { get; set; }

    public DbSet<Trait> Traits { get; set; }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Persona>();
        modelBuilder.Entity<ModelConfig>();
        modelBuilder.Entity<AvailableModel>();
        modelBuilder.Entity<Trait>();
        modelBuilder.Entity<Conversation>();
        modelBuilder.Entity<Message>();
    }
}
