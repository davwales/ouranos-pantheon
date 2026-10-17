namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Token usage of a reply. Input, output and total are summed over every model round;
/// <see cref="ContextTokens"/> is the last round's input plus output, i.e. how much of the
/// model's context window the conversation now fills.
/// </summary>
public sealed record AssistantUsageEvent(
    int InputTokens,
    int OutputTokens,
    int TotalTokens,
    int ContextTokens
) : AssistantEvent;
