using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class ToolCallMessagePartTests
{
    private static readonly Id<MessagePart> PartId = new(Guid.NewGuid().ToString());

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Act
        var part = ToolCallMessagePart.Create(
            PartId,
            "call-1",
            "echo",
            """{"text":"hi"}""",
            AssistantToolStatus.Skipped,
            "skipped",
            true
        );

        // Assert
        part.Id.ShouldBe(PartId);
        part.CallId.ShouldBe("call-1");
        part.Name.ShouldBe("echo");
        part.Arguments.ShouldBe("""{"text":"hi"}""");
        part.Status.ShouldBe(AssistantToolStatus.Skipped);
        part.Output.ShouldBe("skipped");
        part.IsTruncated.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null, "echo")]
    [InlineData(" ", "echo")]
    [InlineData("call-1", null)]
    [InlineData("call-1", " ")]
    public void Create_WhenCallIdOrNameIsBlank_ShouldThrowArgumentException(
        string? callId,
        string? name
    )
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            ToolCallMessagePart.Create(
                PartId,
                callId!,
                name!,
                "{}",
                AssistantToolStatus.Completed,
                "ok"
            )
        );
    }

    [Fact]
    public void Create_WhenStatusIsUndefined_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            ToolCallMessagePart.Create(PartId, "call-1", "echo", "{}", (AssistantToolStatus)99, "")
        );
    }
}
