using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public sealed class TextMessagePart : MessagePart
{
    private TextMessagePart(Id<MessagePart> id)
        : base(id)
    {
        Content = string.Empty;
    }

    public string Content { get; private set; }

    public static TextMessagePart Create(Id<MessagePart> id, string content)
    {
        Guard.Against.NullOrEmpty(content);

        return new TextMessagePart(id) { Content = content };
    }
}
