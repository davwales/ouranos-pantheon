using System.Text;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

/// <summary>
/// Folds a streamed reply into the ordered parts that are saved with it, the same way the chat
/// client folds the stream for display: consecutive deltas merge and a tool result completes the
/// call with the same id.
/// </summary>
internal sealed class MessagePartsBuilder
{
    private readonly List<Draft> _drafts = [];
    private readonly StringBuilder _content = new();

    public string Content => _content.ToString();

    public void Apply(AssistantEvent assistantEvent)
    {
        switch (assistantEvent)
        {
            case AssistantReasoningEvent reasoning:
                Append(DraftKind.Reasoning, reasoning.Content);
                break;
            case AssistantContentEvent content:
                _content.Append(content.Content);
                Append(DraftKind.Text, content.Content);
                break;
            case AssistantToolEvent tool:
                _drafts.Add(
                    new Draft(DraftKind.ToolCall)
                    {
                        CallId = tool.CallId,
                        Name = tool.Name,
                        Arguments = tool.Arguments,
                    }
                );
                break;
            case AssistantToolResultEvent result
                when _drafts.LastOrDefault(d => d.CallId == result.CallId) is { } call:
                call.Status = result.Status;
                call.Output = result.Output;
                call.IsTruncated = result.IsTruncated;
                call.DurationMs = result.DurationMs;
                break;
            case AssistantImageEvent image:
                _drafts.Add(new Draft(DraftKind.Image) { Image = image.Image });
                break;
        }
    }

    public List<MessagePart> Build()
    {
        return [.. _drafts.Select(ToPart)];
    }

    private void Append(DraftKind kind, string delta)
    {
        if (_drafts.Count > 0 && _drafts[^1].Kind == kind)
        {
            _drafts[^1].Text.Append(delta);
            return;
        }

        var draft = new Draft(kind);
        draft.Text.Append(delta);
        _drafts.Add(draft);
    }

    private static MessagePart ToPart(Draft draft)
    {
        var id = DatabaseExtensions.CreateId<MessagePart>();

        return draft.Kind switch
        {
            DraftKind.Reasoning => ReasoningMessagePart.Create(id, draft.Text.ToString()),
            DraftKind.Text => TextMessagePart.Create(id, draft.Text.ToString()),
            DraftKind.ToolCall => ToolCallMessagePart.Create(
                id,
                draft.CallId,
                draft.Name,
                draft.Arguments,
                draft.Status ?? AssistantToolStatus.Failed,
                draft.Output,
                draft.IsTruncated,
                draft.DurationMs
            ),
            DraftKind.Image when draft.Image is { } image => ImageMessagePart.Create(
                id,
                new Id<GeneratedImage>(image.Id),
                image.Width,
                image.Height,
                image.AltText
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(draft), draft.Kind, null),
        };
    }

    private enum DraftKind
    {
        Reasoning,
        Text,
        ToolCall,
        Image,
    }

    private sealed class Draft(DraftKind kind)
    {
        public DraftKind Kind { get; } = kind;

        public StringBuilder Text { get; } = new();

        public string CallId { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Arguments { get; init; } = string.Empty;

        public AssistantToolStatus? Status { get; set; }

        public string Output { get; set; } = string.Empty;

        public bool IsTruncated { get; set; }

        public long? DurationMs { get; set; }

        public AssistantImage? Image { get; init; }
    }
}
