using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

public sealed record Conversation(
    Assistant Assistant,
    List<Message> Messages
)
{
    public List<Message> Messages { get; init; } = Messages ?? [];
}