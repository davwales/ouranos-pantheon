using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;

public sealed class ToolSetAssistantTool(
    IAssistantTool<HermesChatContext> inner,
    string? additionalDescription
) : IAssistantTool<HermesChatContext>
{
    private readonly IAssistantTool<HermesChatContext> _inner = Guard.Against.Null(inner);

    public string Name => _inner.Name;

    public string Description =>
        string.IsNullOrWhiteSpace(additionalDescription)
            ? _inner.Description
            : $"{_inner.Description.TrimEnd()}\n\n{additionalDescription}";

    public BinaryData ParametersSchema => _inner.ParametersSchema;

    public bool IsSingleDeliveryPerReply => _inner.IsSingleDeliveryPerReply;

    public bool IsAvailable(HermesChatContext context)
    {
        return _inner.IsAvailable(context);
    }

    public ValueTask<AssistantToolResult> InvokeAsync(
        HermesChatContext context,
        string arguments,
        CancellationToken cancellationToken
    )
    {
        return _inner.InvokeAsync(context, arguments, cancellationToken);
    }
}
