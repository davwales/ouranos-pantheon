using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// The LLM streaming loop shared by every assistant: streams model rounds over the Responses API,
/// runs the tools the model calls and feeds their output back, and emits the shared event contract.
/// Callers own the instructions, the history and how the turn ends.
/// </summary>
public sealed class AssistantToolLoop<TContext>(
    ILogger logger,
    IOuranosMachineLearningClient mlClient,
    TimeProvider? timeProvider = null
)
    where TContext : class
{
    public const string GenerationFailedMessage = "The assistant failed to generate a response.";

    public const string ToolFailedOutput = "The tool failed; answer without it.";

    public const string TooManyToolCallsOutput =
        "Skipped: too many tool calls at once; answer with what you have.";

    public const string RepeatedCallOutput =
        "Skipped: this reply already has this tool's result; answer the user now.";

    private readonly ILogger _logger = Guard.Against.Null(logger);
    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <summary>
    /// Streams one assistant reply: model rounds with the offered tools until the model answers
    /// without calling one, or the tool rounds run out. Never yields a done event, so callers can
    /// finish the turn themselves.
    /// </summary>
    public async IAsyncEnumerable<AssistantEvent> RunAsync(
        AssistantPrompt prompt,
        string instructions,
        IReadOnlyList<ResponseInputItemDto> history,
        IReadOnlyList<IAssistantTool<TContext>> tools,
        TContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        Guard.Against.Null(prompt);
        Guard.Against.NullOrWhiteSpace(instructions);
        Guard.Against.Null(history);
        Guard.Against.Null(tools);
        Guard.Against.Null(context);

        List<FunctionToolDto> definitions =
        [
            .. tools.Select(t => new FunctionToolDto(t.Name, t.Description, t.ParametersSchema)),
        ];

        List<ResponseInputItemDto> input = [.. history];

        ChatCompletionUsage? usage = null;
        ChatCompletionUsage? lastUsage = null;
        var hasContent = false;
        var delivered = new HashSet<string>();

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
                var (hasNext, error) = await TryMoveNextAsync(stream, cancellationToken);

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
                    lastUsage = roundUsage;
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
                yield return new AssistantToolEvent(call.CallId, call.Name, call.Arguments);

                AssistantToolResult result;
                AssistantToolStatus status;
                long durationMs = 0;

                if (executed >= prompt.MaxToolCallsPerRound)
                {
                    _logger.LogWarning(
                        "Assistant skipped tool '{name}': more than {max} calls in one round.",
                        call.Name,
                        prompt.MaxToolCallsPerRound
                    );
                    result = new AssistantToolResult(TooManyToolCallsOutput);
                    status = AssistantToolStatus.Skipped;
                }
                else if (
                    tool is { IsSingleDeliveryPerReply: true }
                    && delivered.Contains(tool.Name)
                )
                {
                    _logger.LogWarning(
                        "Assistant skipped tool '{name}': it already delivered a result in this reply.",
                        call.Name
                    );
                    result = new AssistantToolResult(RepeatedCallOutput);
                    status = AssistantToolStatus.Skipped;
                }
                else
                {
                    executed++;
                    var startedAt = _timeProvider.GetTimestamp();
                    result = await InvokeToolAsync(tool, context, call, cancellationToken);
                    durationMs = (long)_timeProvider.GetElapsedTime(startedAt).TotalMilliseconds;

                    status = result.IsError
                        ? AssistantToolStatus.Failed
                        : AssistantToolStatus.Completed;

                    // A delivered action or image is an answer in itself, so the model may end the
                    // reply without adding text.
                    if (!result.IsError && (result.Action ?? result.Image) is not null)
                    {
                        delivered.Add(call.Name);
                        hasContent = true;
                    }

                    if (result.Action is not null)
                    {
                        yield return new AssistantActionEvent(
                            call.CallId,
                            call.Name,
                            result.Action
                        );
                    }
                }

                var (output, isTruncated) = Truncate(result.Output, prompt.MaxToolOutputCharacters);

                input.Add(new FunctionCallInputItemDto(call));
                input.Add(new FunctionCallOutputInputItemDto(call.CallId, output));

                yield return new AssistantToolResultEvent(
                    call.CallId,
                    status,
                    output,
                    isTruncated,
                    durationMs
                );

                if (result.Image is not null && !result.IsError)
                {
                    yield return new AssistantImageEvent(call.CallId, result.Image);
                }
            }
        }

        if (!hasContent)
        {
            _logger.LogWarning(
                "Assistant completion finished without producing any content after {outputTokens} output tokens.",
                usage?.OutputTokens
            );
            yield return new AssistantErrorEvent(GenerationFailedMessage);
            yield break;
        }

        if (usage is not null && lastUsage is not null)
        {
            yield return new AssistantUsageEvent(
                usage.InputTokens,
                usage.OutputTokens,
                usage.TotalTokens,
                lastUsage.InputTokens + lastUsage.OutputTokens
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
            return new AssistantToolResult($"Unknown tool '{call.Name}'.", IsError: true);
        }

        try
        {
            return await tool.InvokeAsync(context, call.Arguments, cancellationToken);
        }
        catch (Exception ex) when (!IsCancellation(ex, cancellationToken))
        {
            _logger.LogError(ex, "Assistant tool '{name}' failed.", call.Name);
            return new AssistantToolResult(ToolFailedOutput, IsError: true);
        }
    }

    private static (string Output, bool IsTruncated) Truncate(string output, int maxCharacters)
    {
        return output.Length <= maxCharacters
            ? (output, false)
            : ($"{output[..maxCharacters]}…", true);
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
        IAsyncEnumerator<ResponseStreamChunk> stream,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return (await stream.MoveNextAsync(), null);
        }
        catch (Exception ex) when (!IsCancellation(ex, cancellationToken))
        {
            return (false, ex);
        }
    }

    // HttpClient timeouts also throw TaskCanceledException, so only the caller's token marks a
    // real cancellation; anything else is a failure the user must see.
    private static bool IsCancellation(Exception ex, CancellationToken cancellationToken)
    {
        return ex is OperationCanceledException && cancellationToken.IsCancellationRequested;
    }
}
