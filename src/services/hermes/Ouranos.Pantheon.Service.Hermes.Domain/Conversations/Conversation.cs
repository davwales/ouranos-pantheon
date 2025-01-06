using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

public sealed record Conversation(
    Character User,
    Character Assistant,
    string Context,
    List<Message> Messages
)
{
    public List<Message> Messages { get; init; } = Messages ?? [];
}