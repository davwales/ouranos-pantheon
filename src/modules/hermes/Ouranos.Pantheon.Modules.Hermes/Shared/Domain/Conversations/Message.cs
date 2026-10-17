using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public sealed class Message : BaseEntity<Id<Message>>
{
    private Message(Id<Message> id)
        : base(id)
    {
        Content = string.Empty;
    }

    public Id<Conversation> ConversationId { get; private set; }

    public string Content { get; private set; }

    public Role Role { get; private set; }

    public int SortOrder { get; private set; }

    public List<MessagePart> Parts { get; private set; } = [];

    public static Message Create(
        Id<Message> id,
        Id<Conversation> conversationId,
        string content,
        Role role,
        int sortOrder,
        IEnumerable<MessagePart>? parts = null
    )
    {
        Guard.Against.Null(conversationId);
        Guard.Against.NullOrWhiteSpace(content);

        List<MessagePart> ordered = [.. parts ?? []];
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Order(i);
        }

        return new Message(id)
        {
            ConversationId = conversationId,
            Content = content,
            Role = role,
            SortOrder = sortOrder,
            Parts = ordered,
        };
    }
}
