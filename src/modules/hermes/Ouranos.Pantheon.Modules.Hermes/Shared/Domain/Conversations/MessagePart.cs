using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

/// <summary>
/// One ordered piece of an assistant reply as it streamed: reasoning, text or a tool call. Parts
/// only drive how a reply renders and which tool calls are replayed; <see cref="Message.Content"/>
/// stays the source of truth for the reply's text.
/// </summary>
public abstract class MessagePart(Id<MessagePart> id) : BaseEntity<Id<MessagePart>>(id)
{
    public Id<Message> MessageId { get; private set; }

    public int SortOrder { get; private set; }

    internal void Order(int sortOrder)
    {
        SortOrder = sortOrder;
    }
}
