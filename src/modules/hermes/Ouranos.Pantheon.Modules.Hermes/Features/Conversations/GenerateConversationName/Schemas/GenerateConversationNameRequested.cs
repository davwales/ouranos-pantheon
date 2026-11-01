using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateConversationName.Schemas;

public sealed record GenerateConversationNameRequested(Id<Conversation> ConversationId)
{
    public const string Queue = "hermes-conversation-name";
}
