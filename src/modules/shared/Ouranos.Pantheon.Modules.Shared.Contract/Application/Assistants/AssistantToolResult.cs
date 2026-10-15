namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// What a tool hands back: <see cref="Output"/> goes to the model, and the optional
/// <see cref="Action"/> is streamed to the page.
/// </summary>
public sealed record AssistantToolResult(string Output, object? Action = null);
