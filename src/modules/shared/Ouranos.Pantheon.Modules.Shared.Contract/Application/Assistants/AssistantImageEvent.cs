namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// An image produced by the tool call <see cref="CallId"/>, sent after its result.
/// </summary>
public sealed record AssistantImageEvent(string CallId, AssistantImage Image) : AssistantEvent;
