using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;

public static class MessagePartMapping
{
    public static MessagePart ToEntity(MessagePartDto part)
    {
        return part switch
        {
            ReasoningMessagePartDto reasoning => ReasoningMessagePart.Create(
                DatabaseExtensions.CreateId<MessagePart>(),
                reasoning.Content
            ),
            TextMessagePartDto text => TextMessagePart.Create(
                DatabaseExtensions.CreateId<MessagePart>(),
                text.Content
            ),
            ToolCallMessagePartDto tool => ToolCallMessagePart.Create(
                DatabaseExtensions.CreateId<MessagePart>(),
                tool.CallId,
                tool.Name,
                tool.Arguments,
                tool.Status,
                tool.Output,
                tool.IsTruncated,
                tool.DurationMs
            ),
            ImageMessagePartDto image => ImageMessagePart.Create(
                DatabaseExtensions.CreateId<MessagePart>(),
                image.ImageId,
                image.Width,
                image.Height,
                image.AltText
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, null),
        };
    }

    public static MessagePartDto ToDto(MessagePart part)
    {
        return part switch
        {
            ReasoningMessagePart reasoning => new ReasoningMessagePartDto(reasoning.Content),
            TextMessagePart text => new TextMessagePartDto(text.Content),
            ToolCallMessagePart tool => new ToolCallMessagePartDto(
                tool.CallId,
                tool.Name,
                tool.Arguments,
                tool.Status,
                tool.Output,
                tool.IsTruncated,
                tool.DurationMs
            ),
            ImageMessagePart image => new ImageMessagePartDto(
                image.ImageId,
                image.Width,
                image.Height,
                image.AltText
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, null),
        };
    }
}
