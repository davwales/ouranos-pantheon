using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GenerateCompletion;

public sealed class MessagePartsBuilderTests
{
    [Fact]
    public void Build_WhenDeltasRepeat_ShouldMergeThemIntoOrderedParts()
    {
        // Arrange
        var builder = new MessagePartsBuilder();
        AssistantEvent[] events =
        [
            new AssistantReasoningEvent("Let me "),
            new AssistantReasoningEvent("check."),
            new AssistantContentEvent("One "),
            new AssistantToolEvent("call-1", "echo", """{"text":"hi"}"""),
            new AssistantToolResultEvent("call-1", AssistantToolStatus.Completed, "hi", true),
            new AssistantReasoningEvent("Done."),
            new AssistantContentEvent("two"),
            new AssistantContentEvent(" three"),
            new AssistantUsageEvent(1, 2, 3, 3),
        ];

        // Act
        foreach (var assistantEvent in events)
        {
            builder.Apply(assistantEvent);
        }

        var parts = builder.Build();

        // Assert
        builder.Content.ShouldBe("One two three");
        parts.Count.ShouldBe(5);
        parts[0].ShouldBeOfType<ReasoningMessagePart>().Content.ShouldBe("Let me check.");
        parts[1].ShouldBeOfType<TextMessagePart>().Content.ShouldBe("One ");

        var tool = parts[2].ShouldBeOfType<ToolCallMessagePart>();
        tool.CallId.ShouldBe("call-1");
        tool.Name.ShouldBe("echo");
        tool.Arguments.ShouldBe("""{"text":"hi"}""");
        tool.Status.ShouldBe(AssistantToolStatus.Completed);
        tool.Output.ShouldBe("hi");
        tool.IsTruncated.ShouldBeTrue();
        parts[3].ShouldBeOfType<ReasoningMessagePart>().Content.ShouldBe("Done.");
        parts[4].ShouldBeOfType<TextMessagePart>().Content.ShouldBe("two three");
    }

    [Fact]
    public void Build_WhenToolCallHasNoResult_ShouldMarkItFailed()
    {
        // Arrange
        var builder = new MessagePartsBuilder();
        builder.Apply(new AssistantToolEvent("call-1", "echo", "{}"));

        // Act
        var parts = builder.Build();

        // Assert
        var tool = parts.Single().ShouldBeOfType<ToolCallMessagePart>();
        tool.Status.ShouldBe(AssistantToolStatus.Failed);
        tool.Output.ShouldBeEmpty();
    }

    [Fact]
    public void Apply_WhenResultHasNoMatchingCall_ShouldIgnoreIt()
    {
        // Arrange
        var builder = new MessagePartsBuilder();

        // Act
        builder.Apply(
            new AssistantToolResultEvent("missing", AssistantToolStatus.Completed, "x", false)
        );

        // Assert
        builder.Build().ShouldBeEmpty();
        builder.Content.ShouldBeEmpty();
    }
}
