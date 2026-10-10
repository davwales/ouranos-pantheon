using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Base class for module-owned AI assistants. Subclasses supply the prompt for a strongly-typed
/// context; the base class owns the LLM streaming loop and the shared event contract.
/// </summary>
public abstract class PantheonAssistant<TContext>(
    ILogger logger,
    IOuranosMachineLearningClient mlClient
) : IPantheonStreamHandler<AssistantCompletionInput<TContext>, AssistantEvent>
    where TContext : class
{
    public const string GenerationFailedMessage = "The assistant failed to generate a response.";

    private readonly ILogger _logger = Guard.Against.Null(logger);
    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);

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

        await foreach (
            var assistantEvent in StreamCompletionAsync(prompt, messages, cancellationToken)
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
        List<AssistantMessageInput> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await using var stream = _mlClient
            .StreamResponseAsync(
                prompt.ModelIdentifier,
                $"{prompt.SystemPrompt}\n\n{AssistantFormatting.Instructions}",
                [.. messages.Select(m => new MessageDto(m.Content, MapRole(m.Role)))],
                prompt.Temperature,
                prompt.MaxTokens,
                prompt.ReasoningEffort,
                cancellationToken
            )
            .GetAsyncEnumerator(cancellationToken);

        ChatCompletionUsage? usage = null;
        var hasContent = false;

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

            usage = stream.Current.Usage ?? usage;
        }

        // Reasoning counts against the output token budget, so a model can finish without ever
        // answering; that is a failure, not an empty reply.
        if (!hasContent)
        {
            _logger.LogWarning("Assistant completion finished without producing any content.");
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
