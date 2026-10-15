using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Base class for a tool an assistant can call. The arguments record doubles as the JSON schema
/// the model fills in, so optional arguments are nullable properties.
/// </summary>
public abstract class AssistantTool<TContext, TArguments>(ILogger logger) : IAssistantTool<TContext>
    where TContext : class
    where TArguments : class
{
    public const string InvalidArgumentsOutput =
        "Invalid arguments: call the tool again with arguments that match its schema.";

    private readonly ILogger _logger = Guard.Against.Null(logger);

    public abstract string Name { get; }

    public abstract string Description { get; }

    public BinaryData ParametersSchema => StructuredOutputSchema.For<TArguments>();

    public virtual bool IsAvailable(TContext context)
    {
        return true;
    }

    public async ValueTask<AssistantToolResult> InvokeAsync(
        TContext context,
        string arguments,
        CancellationToken cancellationToken
    )
    {
        _logger.LogTrace(
            "Attempting to invoke assistant tool '{name}' with arguments '{arguments}'.",
            Name,
            arguments
        );
        cancellationToken.ThrowIfCancellationRequested();

        // Models often send nothing at all for a tool without parameters.
        var parsed = StructuredCompletionParser.Parse<TArguments>(
            string.IsNullOrWhiteSpace(arguments) ? "{}" : arguments
        );

        if (parsed is null)
        {
            _logger.LogWarning(
                "Assistant tool '{name}' received invalid arguments '{arguments}'.",
                Name,
                arguments
            );
            return new AssistantToolResult(InvalidArgumentsOutput);
        }

        var result = await ExecuteAsync(context, parsed, cancellationToken);

        _logger.LogDebug("Successfully invoked assistant tool '{name}'.", Name);
        return result;
    }

    protected abstract ValueTask<AssistantToolResult> ExecuteAsync(
        TContext context,
        TArguments arguments,
        CancellationToken cancellationToken
    );
}
