using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;

public sealed record CompactConversationMessageInput(string Content, Role Role);
