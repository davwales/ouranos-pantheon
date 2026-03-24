using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record ConversationInput(
    ModelInput Model,
    PersonaInput Persona,
    List<Message> Messages,
    List<TraitInput>? Traits = null
);