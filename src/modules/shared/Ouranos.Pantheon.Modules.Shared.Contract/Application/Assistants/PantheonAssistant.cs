using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Base class for module-owned page assistants. Subclasses supply the prompt for a strongly-typed
/// context and the tools registered for that context; the base class trims the client-sent history,
/// adds the shared panel formatting and runs the turn through <see cref="AssistantToolLoop{TContext}"/>.
/// </summary>
public abstract class PantheonAssistant<TContext>(
    ILogger logger,
    IOuranosMachineLearningClient mlClient,
    IEnumerable<IAssistantTool<TContext>> tools
) : IPantheonStreamHandler<AssistantCompletionInput<TContext>, AssistantEvent>
    where TContext : class
{
    private readonly ILogger _logger = Guard.Against.Null(logger);
    private readonly AssistantToolLoop<TContext> _loop = new(logger, Guard.Against.Null(mlClient));
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
        var messages = TrimHistory(
            [.. input.Messages.Select(Render)],
            prompt.HistoryCharacterBudget
        );
        var tools = _tools.Where(t => t.IsAvailable(input.Context)).ToList();

        await foreach (
            var assistantEvent in _loop.RunAsync(
                prompt,
                ComposeInstructions(prompt, tools),
                [
                    .. messages.Select(m => new MessageInputItemDto(
                        new MessageDto(m.Content, MapRole(m.Role))
                    )),
                ],
                tools,
                input.Context,
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

    private static string ComposeInstructions(
        AssistantPrompt prompt,
        List<IAssistantTool<TContext>> tools
    )
    {
        return tools.Count > 0
            ? $"{prompt.SystemPrompt}\n\n{AssistantFormatting.Instructions}\n\n{AssistantFormatting.ToolInstructions}"
            : $"{prompt.SystemPrompt}\n\n{AssistantFormatting.Instructions}";
    }

    // Pages apply proposed actions without telling the server, so the client reports what the
    // user did with them and the model reads it as a note on the reply that proposed them.
    private static AssistantMessageInput Render(AssistantMessageInput message)
    {
        if (message.Actions is not { Count: > 0 } actions)
        {
            return message;
        }

        var outcomes = string.Join(
            "; ",
            actions.Select(a =>
                string.IsNullOrWhiteSpace(a.Description)
                    ? $"{a.Name}: {Describe(a.Status)}"
                    : $"\"{a.Description}\" ({a.Name}): {Describe(a.Status)}"
            )
        );

        return message with
        {
            Content = $"{message.Content}\n\n[Proposals: {outcomes}]",
            Actions = null,
        };
    }

    private static string Describe(AssistantActionStatus status)
    {
        return status switch
        {
            AssistantActionStatus.Pending => "not acted on yet",
            AssistantActionStatus.Applied => "applied by the user",
            AssistantActionStatus.Rejected => "rejected by the user",
            AssistantActionStatus.Failed => "failed to apply",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }

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
