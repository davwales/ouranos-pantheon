namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public sealed record AssistantUsageEvent(int InputTokens, int OutputTokens, int TotalTokens)
    : AssistantEvent;
