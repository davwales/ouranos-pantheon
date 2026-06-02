using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Converters;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Database.ModelConfigurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasIdConversion();
        builder.Property(c => c.PersonaId).HasIdConversion();
        builder.Property(c => c.ModelConfigId).HasIdConversion();

        builder
            .HasOne(c => c.Persona)
            .WithMany()
            .HasForeignKey(c => c.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(c => c.ModelConfig)
            .WithMany()
            .HasForeignKey(c => c.ModelConfigId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(c => c.Traits).WithMany();
        builder
            .HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(c => c.FolderId).HasConversion<IdConverter<Folder>>();
        builder
            .HasOne(c => c.Folder)
            .WithMany(f => f.Conversations)
            .HasForeignKey(c => c.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
