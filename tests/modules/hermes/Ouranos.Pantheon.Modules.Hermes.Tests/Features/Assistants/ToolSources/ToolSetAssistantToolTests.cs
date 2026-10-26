using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Assistants.ToolSources;

public sealed class ToolSetAssistantToolTests
{
    private readonly IAssistantTool<HermesChatContext> _inner = Substitute.For<
        IAssistantTool<HermesChatContext>
    >();

    public ToolSetAssistantToolTests()
    {
        _inner.Name.Returns("echo");
        _inner.Description.Returns("Echoes text.  ");
    }

    [Fact]
    public void Constructor_WhenInnerNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IAssistantTool<HermesChatContext> inner = null!;

        // Act
        var create = () => new ToolSetAssistantTool(inner, null);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Name_ShouldReturnInnerName()
    {
        // Arrange
        var tool = new ToolSetAssistantTool(_inner, "Be brief.");

        // Act
        var result = tool.Name;

        // Assert
        result.ShouldBe("echo");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Description_WhenAdditionalDescriptionBlank_ShouldReturnInnerDescription(
        string? additionalDescription
    )
    {
        // Arrange
        var tool = new ToolSetAssistantTool(_inner, additionalDescription);

        // Act
        var result = tool.Description;

        // Assert
        result.ShouldBe("Echoes text.  ");
    }

    [Fact]
    public void Description_WhenAdditionalDescriptionProvided_ShouldAppendItAfterBlankLine()
    {
        // Arrange
        var tool = new ToolSetAssistantTool(_inner, "Be brief.");

        // Act
        var result = tool.Description;

        // Assert
        result.ShouldBe("Echoes text.\n\nBe brief.");
    }

    [Fact]
    public void ParametersSchema_ShouldReturnInnerSchema()
    {
        // Arrange
        var schema = BinaryData.FromString("{\"type\":\"object\"}");
        _inner.ParametersSchema.Returns(schema);
        var tool = new ToolSetAssistantTool(_inner, null);

        // Act
        var result = tool.ParametersSchema;

        // Assert
        result.ShouldBeSameAs(schema);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsSingleDeliveryPerReply_ShouldReturnInnerValue(bool expected)
    {
        // Arrange
        _inner.IsSingleDeliveryPerReply.Returns(expected);
        var tool = new ToolSetAssistantTool(_inner, null);

        // Act
        var result = tool.IsSingleDeliveryPerReply;

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAvailable_ShouldDelegateToInner(bool expected)
    {
        // Arrange
        var context = new HermesChatContext(null, [], ConversationImages.Empty);
        _inner.IsAvailable(context).Returns(expected);
        var tool = new ToolSetAssistantTool(_inner, null);

        // Act
        var result = tool.IsAvailable(context);

        // Assert
        result.ShouldBe(expected);
        _inner.Received(1).IsAvailable(context);
    }

    [Fact]
    public async Task InvokeAsync_ShouldDelegateToInner()
    {
        // Arrange
        var context = new HermesChatContext(null, [], ConversationImages.Empty);
        var expected = new AssistantToolResult("done");
        var ct = CancellationToken.None;
        _inner.InvokeAsync(context, "{}", ct).Returns(ValueTask.FromResult(expected));
        var tool = new ToolSetAssistantTool(_inner, "Be brief.");

        // Act
        var result = await tool.InvokeAsync(context, "{}", ct);

        // Assert
        result.ShouldBeSameAs(expected);
        await _inner.Received(1).InvokeAsync(context, "{}", ct);
    }
}
