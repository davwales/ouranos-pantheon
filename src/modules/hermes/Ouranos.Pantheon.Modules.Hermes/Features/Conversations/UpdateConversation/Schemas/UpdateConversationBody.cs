using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;

public sealed record UpdateConversationBody(
    string Name,
    Id<Persona> PersonaId,
    Id<ModelConfig> ModelConfigId,
    Id<Trait>[] TraitIds,
    List<UpdateConversationMessageInput> Messages,
    bool IsPublic,
    Id<Folder>? FolderId = null
);
