using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public sealed class ReasoningMessagePart : MessagePart
{
    private ReasoningMessagePart(Id<MessagePart> id)
        : base(id)
    {
        Content = string.Empty;
    }

    public string Content { get; private set; }

    public static ReasoningMessagePart Create(Id<MessagePart> id, string content)
    {
        Guard.Against.NullOrEmpty(content);

        return new ReasoningMessagePart(id) { Content = content };
    }
}
