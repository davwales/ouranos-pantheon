using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.Schemas;

public sealed class MessagePartMappingTests
{
    public static TheoryData<MessagePartDto> Parts =>
        new()
        {
            new ReasoningMessagePartDto("Thinking"),
            new TextMessagePartDto("Hello"),
            new ToolCallMessagePartDto(
                "call-1",
                "echo",
                """{"text":"hi"}""",
                AssistantToolStatus.Failed,
                "boom",
                true
            ),
        };

    [Theory]
    [MemberData(nameof(Parts))]
    public void ToEntity_ThenToDto_ShouldRoundTrip(MessagePartDto part)
    {
        // Act
        var entity = MessagePartMapping.ToEntity(part);
        var result = MessagePartMapping.ToDto(entity);

        // Assert
        result.ShouldBe(part);
    }

    [Fact]
    public void ToEntity_WhenToolCall_ShouldCreateToolCallPart()
    {
        // Act
        var entity = MessagePartMapping.ToEntity(
            new ToolCallMessagePartDto(
                "call-1",
                "echo",
                "{}",
                AssistantToolStatus.Completed,
                "ok",
                false
            )
        );

        // Assert
        entity.ShouldBeOfType<ToolCallMessagePart>().CallId.ShouldBe("call-1");
    }
}
