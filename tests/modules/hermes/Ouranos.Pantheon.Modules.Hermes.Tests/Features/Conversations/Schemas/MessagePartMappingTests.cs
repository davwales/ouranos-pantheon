using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

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
                true,
                1500
            ),
            new ImageMessagePartDto(
                new Id<GeneratedImage>("0f8fad5b-d9cb-469f-a165-70867728950e"),
                64,
                32,
                "A fox"
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
