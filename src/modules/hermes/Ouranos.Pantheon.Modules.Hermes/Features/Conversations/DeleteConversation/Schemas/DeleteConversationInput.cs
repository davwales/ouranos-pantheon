using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation.Schemas;

public sealed record DeleteConversationInput(Id<Conversation> ConversationId);
