using Microsoft.Extensions.Logging;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// A tool that hands the model typed data rather than prose. The tool owns how its query results
/// map onto <typeparamref name="TData"/>; serialization keeps the JSON compact for the limited
/// context window.
/// </summary>
public abstract class AssistantDataTool<TContext, TArguments, TData>(ILogger logger)
    : AssistantTool<TContext, TArguments>(logger)
    where TContext : class
    where TArguments : class
    where TData : class
{
    protected sealed override async ValueTask<AssistantToolResult> ExecuteAsync(
        TContext context,
        TArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var data = await QueryAsync(context, arguments, cancellationToken);

        return new AssistantToolResult(AssistantToolJson.Serialize(data));
    }

    protected abstract ValueTask<AssistantData<TData>> QueryAsync(
        TContext context,
        TArguments arguments,
        CancellationToken cancellationToken
    );
}
