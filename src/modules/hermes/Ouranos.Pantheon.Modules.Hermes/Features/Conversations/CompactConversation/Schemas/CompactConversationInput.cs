using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;

public sealed record CompactConversationInput(
    Id<Conversation>? ConversationId,
    string ModelIdentifier,
    string SystemPrompt,
    string PersonaName,
    string PersonaDescription,
    List<CompactConversationMessageInput> Messages
);
