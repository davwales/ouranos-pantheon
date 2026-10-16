using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Base class for module-owned AI assistants. Subclasses supply the prompt for a strongly-typed
/// context and the tools registered for that context; the base class owns the LLM streaming loop,
/// the tool rounds and the shared event contract.
/// </summary>
public abstract class PantheonAssistant<TContext>(
    ILogger logger,
    IOuranosMachineLearningClient mlClient,
    IEnumerable<IAssistantTool<TContext>> tools
) : IPantheonStreamHandler<AssistantCompletionInput<TContext>, AssistantEvent>
    where TContext : class
{
    public const string GenerationFailedMessage = "The assistant failed to generate a response.";

    public const string ToolFailedOutput = "The tool failed; answer without it.";

    public const string TooManyToolCallsOutput =
        "Skipped: too many tool calls at once; answer with what you have.";

    public const string RepeatedActionOutput =
        "Skipped: this reply already proposed this change; answer the user now.";

    private readonly ILogger _logger = Guard.Against.Null(logger);
    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);
    private readonly IReadOnlyList<IAssistantTool<TContext>> _tools =
    [
        .. Guard.Against.Null(tools),
    ];

    public async IAsyncEnumerable<AssistantEvent> Handle(
        AssistantCompletionInput<TContext> input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle assistant completion '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.Null(input.Messages);
        Guard.Against.Null(input.Context);

        var prompt = await BuildPromptAsync(input.Context, cancellationToken);
        var messages = TrimHistory(input.Messages, prompt.HistoryCharacterBudget);
        var tools = _tools.Where(t => t.IsAvailable(input.Context)).ToList();

        await foreach (
            var assistantEvent in StreamCompletionAsync(
                prompt,
                input.Context,
                tools,
                messages,
                cancellationToken
            )
        )
        {
            yield return assistantEvent;

            if (assistantEvent is AssistantErrorEvent)
            {
                yield break;
            }
        }

        yield return new AssistantDoneEvent();

        _logger.LogDebug("Successfully handled assistant completion.");
    }

    protected abstract ValueTask<AssistantPrompt> BuildPromptAsync(
        TContext context,
        CancellationToken cancellationToken
    );

    // The client resends the whole conversation every turn, so older turns are dropped once they
    // no longer fit beside the system prompt, the reasoning and the reply.
    private List<AssistantMessageInput> TrimHistory(
        List<AssistantMessageInput> messages,
        int characterBudget
    )
    {
        var kept = new List<AssistantMessageInput>();
        var used = 0;

        for (var i = messages.Count - 1; i >= 0; i--)
        {
            used += messages[i].Content.Length;

            if (kept.Count > 0 && used > characterBudget)
            {
                break;
            }

            kept.Add(messages[i]);
        }

        kept.Reverse();

        while (kept.Count > 1 && kept[0].Role == AssistantRole.Assistant)
        {
            kept.RemoveAt(0);
        }

        if (kept.Count < messages.Count)
        {
            _logger.LogDebug(
                "Trimmed {droppedCount} of {messageCount} history messages to fit the context budget.",
                messages.Count - kept.Count,
                messages.Count
            );
        }

        return kept;
    }

    private async IAsyncEnumerable<AssistantEvent> StreamCompletionAsync(
        AssistantPrompt prompt,
        TContext context,
        List<IAssistantTool<TContext>> tools,
        List<AssistantMessageInput> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var instructions =
            tools.Count > 0
                ? $"{prompt.SystemPrompt}\n\n{AssistantFormatting.Instructions}\n\n{AssistantFormatting.ToolInstructions}"
                : $"{prompt.SystemPrompt}\n\n{AssistantFormatting.Instructions}";

        List<FunctionToolDto> definitions =
        [
            .. tools.Select(t => new FunctionToolDto(t.Name, t.Description, t.ParametersSchema)),
        ];

        List<ResponseInputItemDto> input =
        [
            .. messages.Select(m => new MessageInputItemDto(
                new MessageDto(m.Content, MapRole(m.Role))
            )),
        ];

        ChatCompletionUsage? usage = null;
        var hasContent = false;
        var proposed = new HashSet<string>();

        for (var round = 0; ; round++)
        {
            List<FunctionToolDto> offered = round < prompt.MaxToolRounds ? definitions : [];
            var calls = new List<FunctionCallDto>();

            await using var stream = _mlClient
                .StreamResponseAsync(
                    prompt.ModelIdentifier,
                    instructions,
                    [.. input],
                    offered,
                    prompt.Temperature,
                    prompt.MaxTokens,
                    prompt.ReasoningEffort,
                    cancellationToken
                )
                .GetAsyncEnumerator(cancellationToken);

            while (true)
            {
                var (hasNext, error) = await TryMoveNextAsync(stream);

                if (error is not null)
                {
                    _logger.LogError(error, "Assistant completion stream failed.");
                    yield return new AssistantErrorEvent(GenerationFailedMessage);
                    yield break;
                }

                if (!hasNext)
                {
                    break;
                }

                if (stream.Current.Reasoning is { Length: > 0 } reasoning)
                {
                    yield return new AssistantReasoningEvent(reasoning);
                }

                if (stream.Current.Text is { Length: > 0 } text)
                {
                    hasContent = true;
                    yield return new AssistantContentEvent(text);
                }

                if (stream.Current.FunctionCall is { } call)
                {
                    calls.Add(call);
                }

                if (stream.Current.Usage is { } roundUsage)
                {
                    usage = Add(usage, roundUsage);
                }
            }

            if (calls.Count == 0 || offered.Count == 0)
            {
                break;
            }

            var executed = 0;
            foreach (var call in calls)
            {
                var tool = tools.FirstOrDefault(t => t.Name == call.Name);
                AssistantToolResult result;

                if (executed >= prompt.MaxToolCallsPerRound)
                {
                    _logger.LogWarning(
                        "Assistant skipped tool '{name}': more than {max} calls in one round.",
                        call.Name,
                        prompt.MaxToolCallsPerRound
                    );
                    result = new AssistantToolResult(TooManyToolCallsOutput);
                }
                else if (tool is { IsSingleActionPerReply: true } && proposed.Contains(tool.Name))
                {
                    _logger.LogWarning(
                        "Assistant skipped tool '{name}': it already proposed an action in this reply.",
                        call.Name
                    );
                    result = new AssistantToolResult(RepeatedActionOutput);
                }
                else
                {
                    executed++;
                    yield return new AssistantToolEvent(call.Name);

                    result = await InvokeToolAsync(tool, context, call, cancellationToken);

                    if (result.Action is not null)
                    {
                        proposed.Add(call.Name);
                        yield return new AssistantActionEvent(call.Name, result.Action);
                    }
                }

                input.Add(new FunctionCallInputItemDto(call));
                input.Add(
                    new FunctionCallOutputInputItemDto(
                        call.CallId,
                        Truncate(result.Output, prompt.MaxToolOutputCharacters)
                    )
                );
            }
        }

        // Reasoning counts against the output token budget, so a model can finish without ever
        // answering; that is a failure, not an empty reply.
        if (!hasContent)
        {
            _logger.LogWarning(
                "Assistant completion finished without producing any content after {outputTokens} output tokens.",
                usage?.OutputTokens
            );
            yield return new AssistantErrorEvent(GenerationFailedMessage);
            yield break;
        }

        if (usage is not null)
        {
            yield return new AssistantUsageEvent(
                usage.InputTokens,
                usage.OutputTokens,
                usage.TotalTokens
            );
        }
    }

    private async Task<AssistantToolResult> InvokeToolAsync(
        IAssistantTool<TContext>? tool,
        TContext context,
        FunctionCallDto call,
        CancellationToken cancellationToken
    )
    {
        if (tool is null)
        {
            _logger.LogWarning("Assistant called unknown tool '{name}'.", call.Name);
            return new AssistantToolResult($"Unknown tool '{call.Name}'.");
        }

        try
        {
            return await tool.InvokeAsync(context, call.Arguments, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Assistant tool '{name}' failed.", call.Name);
            return new AssistantToolResult(ToolFailedOutput);
        }
    }

    private static string Truncate(string output, int maxCharacters)
    {
        return output.Length <= maxCharacters ? output : $"{output[..maxCharacters]}…";
    }

    private static ChatCompletionUsage Add(ChatCompletionUsage? total, ChatCompletionUsage round)
    {
        return total is null
            ? round
            : new ChatCompletionUsage(
                total.InputTokens + round.InputTokens,
                total.OutputTokens + round.OutputTokens,
                total.TotalTokens + round.TotalTokens
            );
    }

    // C# forbids yield inside a try with a catch, so stream failures are returned instead of
    // thrown and turned into an error event by the caller.
    private static async Task<(bool HasNext, Exception? Error)> TryMoveNextAsync(
        IAsyncEnumerator<ResponseStreamChunk> stream
    )
    {
        try
        {
            return (await stream.MoveNextAsync(), null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return (false, ex);
        }
    }

    private static RoleDto MapRole(AssistantRole role)
    {
        return role switch
        {
            AssistantRole.User => RoleDto.User,
            AssistantRole.Assistant => RoleDto.Assistant,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
        };
    }
}
