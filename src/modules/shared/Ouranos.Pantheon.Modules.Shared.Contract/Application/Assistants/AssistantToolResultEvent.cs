namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// The outcome of a tool call. <see cref="Output"/> is exactly what the model received, so it is
/// cut short when <see cref="IsTruncated"/> is set.
/// </summary>
public sealed record AssistantToolResultEvent(
    string CallId,
    AssistantToolStatus Status,
    string Output,
    bool IsTruncated
) : AssistantEvent;
