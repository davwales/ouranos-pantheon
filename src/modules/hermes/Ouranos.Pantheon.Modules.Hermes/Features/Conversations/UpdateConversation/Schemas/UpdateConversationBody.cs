using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;

public sealed record UpdateConversationBody(
    string Name,
    Id<Persona> PersonaId,
    Id<ModelConfig> ModelConfigId,
    Id<Trait>[] TraitIds,
    bool IsPublic,
    List<UpdateConversationMessageInput>? Messages = null,
    Id<Folder>? FolderId = null,
    ReasoningEffort? ReasoningEffort = null,
    Id<Lora>[]? LoraIds = null,
    Id<ToolSet>? ToolSetId = null
);
