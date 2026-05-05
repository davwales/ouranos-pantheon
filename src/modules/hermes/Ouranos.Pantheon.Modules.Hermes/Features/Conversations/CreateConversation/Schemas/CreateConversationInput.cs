using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;

public sealed record CreateConversationInput(
    Id<Persona> PersonaId,
    Id<ModelConfig> ModelConfigId,
    Id<Trait>[] TraitIds,
    List<CreateConversationMessageInput> Messages,
    string? Name = null,
    bool IsPublic = true,
    int? InputTokenCount = null,
    int? OutputTokenCount = null,
    int? TotalTokenCount = null
);
