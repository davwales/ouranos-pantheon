using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record ConversationInput(
    ModelInput Model,
    PersonaInput Persona,
    List<CompletionMessageInput> Messages,
    List<TraitInput>? Traits = null,
    ReasoningEffort? ReasoningEffort = null,
    List<LoraInput>? Loras = null
);
