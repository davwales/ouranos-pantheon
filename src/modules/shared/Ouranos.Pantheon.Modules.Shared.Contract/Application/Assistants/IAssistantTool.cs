namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public interface IAssistantTool<in TContext>
    where TContext : class
{
    string Name { get; }

    string Description { get; }

    BinaryData ParametersSchema { get; }

    /// <summary>
    /// When true, a reply carries at most one action from this tool: once it returns an action,
    /// later calls in the same reply are skipped. Calls that return no action, such as invalid
    /// arguments, can still be retried.
    /// </summary>
    bool IsSingleActionPerReply { get; }

    bool IsAvailable(TContext context);

    ValueTask<AssistantToolResult> InvokeAsync(
        TContext context,
        string arguments,
        CancellationToken cancellationToken
    );
}
