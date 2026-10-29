using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database;

public sealed class HermesDbContext(DbContextOptions<HermesDbContext> options)
    : OuranosDbContext(options, "hermes")
{
    public DbSet<Persona> Personas { get; set; }

    public DbSet<ModelConfig> ModelConfigs { get; set; }

    public DbSet<AvailableModel> AvailableModels { get; set; }

    public DbSet<Trait> Traits { get; set; }

    public DbSet<Lora> Loras { get; set; }

    public DbSet<AvailableLora> AvailableLoras { get; set; }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<Folder> Folders { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<MessagePart> MessageParts { get; set; }

    public DbSet<GeneratedImage> GeneratedImages { get; set; }

    public DbSet<ToolSet> ToolSets { get; set; }

    public DbSet<ToolSetTool> ToolSetTools { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Persona>();
        modelBuilder.Entity<ModelConfig>();
        modelBuilder.Entity<AvailableModel>();
        modelBuilder.Entity<Trait>();
        modelBuilder.Entity<Lora>();
        modelBuilder.Entity<AvailableLora>();
        modelBuilder.Entity<Conversation>();
        modelBuilder.Entity<Folder>();
        modelBuilder.Entity<Message>();
        modelBuilder.Entity<MessagePart>();
        modelBuilder.Entity<GeneratedImage>();
        modelBuilder.Entity<ToolSet>();
        modelBuilder.Entity<ToolSetTool>();
    }
}
