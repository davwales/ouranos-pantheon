namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// What the user did with an action an earlier reply proposed, as described on their page.
/// </summary>
public sealed record AssistantActionOutcome(
    string Name,
    string Description,
    AssistantActionStatus Status
);
