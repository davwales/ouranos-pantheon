namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public sealed record Conversation(
    List<Message>? Messages
)
{
    public List<Message> Messages { get; init; } = Messages ?? [];
}
