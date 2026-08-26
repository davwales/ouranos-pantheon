using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;

public sealed record CreateConversationResponse(Id<Conversation> Id, string Name);
