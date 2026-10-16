namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// A tool call the model made, with its raw JSON <see cref="Arguments"/>. Its outcome follows as
/// an <see cref="AssistantToolResultEvent"/> with the same <see cref="CallId"/>.
/// </summary>
public sealed record AssistantToolEvent(string CallId, string Name, string Arguments)
    : AssistantEvent;
