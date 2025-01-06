using Ouranos.Pantheon.Service.Aphrodite.Domain.Characters;

namespace Ouranos.Pantheon.Service.Aphrodite.Domain.Conversations;

public sealed record Conversation(
    Character User,
    Character Assistant,
    string Context,
    List<Message> Messages
)
{
    public List<Message> Messages { get; init; } = Messages ?? [];
}