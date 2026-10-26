using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;

/// <summary>
/// A Hermes chat turn. The conversation id is null while the chat has not been saved yet.
/// </summary>
public interface IConversationScope
{
    Id<Conversation>? ConversationId { get; }

    IReadOnlyList<ImageLora> ImageLoras { get; }

    ConversationImages Images { get; }
}
