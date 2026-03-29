namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record ConversationInput(
    ModelInput Model,
    PersonaInput Persona,
    List<CompletionMessageInput> Messages,
    List<TraitInput>? Traits = null
);
