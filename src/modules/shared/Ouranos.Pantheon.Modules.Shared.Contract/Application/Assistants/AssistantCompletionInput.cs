namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public sealed record AssistantCompletionInput<TContext>(
    List<AssistantMessageInput> Messages,
    TContext Context
)
    where TContext : class;
