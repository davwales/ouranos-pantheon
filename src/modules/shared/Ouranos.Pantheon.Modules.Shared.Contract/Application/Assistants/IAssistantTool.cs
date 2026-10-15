namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public interface IAssistantTool<in TContext>
    where TContext : class
{
    string Name { get; }

    string Description { get; }

    BinaryData ParametersSchema { get; }

    bool IsAvailable(TContext context);

    ValueTask<AssistantToolResult> InvokeAsync(
        TContext context,
        string arguments,
        CancellationToken cancellationToken
    );
}
