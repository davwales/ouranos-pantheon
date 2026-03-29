using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;

public sealed record GetConversationInput(
    Id<Conversation> ConversationId
);
