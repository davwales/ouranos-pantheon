namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// What a tool hands back: <see cref="Output"/> goes to the model, and the optional
/// <see cref="Action"/> and <see cref="Image"/> are streamed to the client. <see cref="IsError"/>
/// marks an output that explains why the call could not be served.
/// </summary>
public sealed record AssistantToolResult(
    string Output,
    object? Action = null,
    bool IsError = false,
    AssistantImage? Image = null
);
