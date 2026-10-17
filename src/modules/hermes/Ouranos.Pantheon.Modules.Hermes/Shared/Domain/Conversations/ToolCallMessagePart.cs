using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public sealed class ToolCallMessagePart : MessagePart
{
    private ToolCallMessagePart(Id<MessagePart> id)
        : base(id)
    {
        CallId = string.Empty;
        Name = string.Empty;
        Arguments = string.Empty;
        Output = string.Empty;
    }

    public string CallId { get; private set; }

    public string Name { get; private set; }

    public string Arguments { get; private set; }

    public AssistantToolStatus Status { get; private set; }

    public string Output { get; private set; }

    public bool IsTruncated { get; private set; }

    public static ToolCallMessagePart Create(
        Id<MessagePart> id,
        string callId,
        string name,
        string arguments,
        AssistantToolStatus status,
        string output,
        bool isTruncated = false
    )
    {
        Guard.Against.NullOrWhiteSpace(callId);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(arguments);
        Guard.Against.EnumOutOfRange(status);
        Guard.Against.Null(output);

        return new ToolCallMessagePart(id)
        {
            CallId = callId,
            Name = name,
            Arguments = arguments,
            Status = status,
            Output = output,
            IsTruncated = isTruncated,
        };
    }
}
