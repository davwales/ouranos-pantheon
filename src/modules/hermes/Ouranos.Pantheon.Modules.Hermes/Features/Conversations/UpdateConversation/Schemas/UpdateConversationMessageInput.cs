using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;

public sealed record UpdateConversationMessageInput(
    string Content,
    Role Role
);
