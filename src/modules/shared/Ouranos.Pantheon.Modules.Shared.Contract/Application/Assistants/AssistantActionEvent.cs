namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// An action for the page to apply. <see cref="Action"/> is a module-owned record that is
/// serialized by its runtime type; the client narrows on <see cref="Name"/>.
/// </summary>
public sealed record AssistantActionEvent(string Name, object Action) : AssistantEvent;
