using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

namespace Ouranos.Pantheon.Hermes.Service.Domain.Conversations;

public sealed record Conversation(
    Assistant Assistant,
    List<Message> Messages
)
{
    public List<Message> Messages { get; init; } = Messages ?? [];
}