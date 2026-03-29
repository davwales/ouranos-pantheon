using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;

public sealed record CreateConversationMessageInput(
    string Content,
    Role Role
);
