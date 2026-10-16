namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// What a tool hands back: <see cref="Output"/> goes to the model, and the optional
/// <see cref="Action"/> is streamed to the page. <see cref="IsError"/> marks an output that
/// explains why the call could not be served.
/// </summary>
public sealed record AssistantToolResult(
    string Output,
    object? Action = null,
    bool IsError = false
);
