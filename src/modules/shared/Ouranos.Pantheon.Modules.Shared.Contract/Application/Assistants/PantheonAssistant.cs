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

        await foreach (
            var assistantEvent in StreamCompletionAsync(prompt, input.Messages, cancellationToken)
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

    private async IAsyncEnumerable<AssistantEvent> StreamCompletionAsync(
        AssistantPrompt prompt,
        List<AssistantMessageInput> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await using var stream = _mlClient
            .StreamChatCompletionAsync(
                prompt.ModelIdentifier,
                ComposeMessages(prompt, messages),
                prompt.Temperature,
                prompt.MaxTokens,
                cancellationToken: cancellationToken
            )
            .GetAsyncEnumerator(cancellationToken);

        ChatCompletionUsage? usage = null;

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

            if (stream.Current.Text is { Length: > 0 } text)
            {
                yield return new AssistantContentEvent(text);
            }

            usage = stream.Current.Usage ?? usage;
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
        IAsyncEnumerator<ChatCompletionChunk> stream
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

    private static List<MessageDto> ComposeMessages(
        AssistantPrompt prompt,
        List<AssistantMessageInput> messages
    )
    {
        return
        [
            new(prompt.SystemPrompt, RoleDto.System),
            .. messages.Select(m => new MessageDto(m.Content, MapRole(m.Role))),
        ];
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
