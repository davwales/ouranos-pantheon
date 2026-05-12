using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;

public sealed record CompactCompleteResponse(Id<Message>? SummaryMessageId)
    : CompactConversationResponse;
