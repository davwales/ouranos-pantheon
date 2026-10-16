namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// An action for the page to apply, proposed by the tool call <see cref="CallId"/>.
/// <see cref="Action"/> is a module-owned record that is serialized by its runtime type; the
/// client narrows on <see cref="Name"/>.
/// </summary>
public sealed record AssistantActionEvent(string CallId, string Name, object Action)
    : AssistantEvent;
