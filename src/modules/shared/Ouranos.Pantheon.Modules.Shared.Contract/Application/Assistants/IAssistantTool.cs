namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public interface IAssistantTool<in TContext>
    where TContext : class
{
    string Name { get; }

    string Description { get; }

    BinaryData ParametersSchema { get; }

    /// <summary>
    /// When true, the tool delivers at most once per reply: once a call returns an action or an
    /// image, later calls in the same reply are skipped. Calls that deliver nothing, such as
    /// invalid arguments, can still be retried.
    /// </summary>
    bool IsSingleDeliveryPerReply { get; }

    bool IsAvailable(TContext context);

    ValueTask<AssistantToolResult> InvokeAsync(
        TContext context,
        string arguments,
        CancellationToken cancellationToken
    );
}
