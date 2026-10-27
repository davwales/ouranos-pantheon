using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record ConversationInput(
    ModelInput Model,
    PersonaInput Persona,
    List<CompletionMessageInput> Messages,
    List<TraitInput>? Traits = null,
    ReasoningEffort? ReasoningEffort = null,
    List<Id<Lora>>? LoraIds = null,
    Id<ToolSet>? ToolSetId = null
);
