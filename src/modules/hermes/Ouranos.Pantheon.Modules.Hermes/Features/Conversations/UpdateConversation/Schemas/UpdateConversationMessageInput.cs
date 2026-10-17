using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;

public sealed record UpdateConversationMessageInput(
    string Content,
    Role Role,
    IReadOnlyList<MessagePartDto>? Parts = null
);
