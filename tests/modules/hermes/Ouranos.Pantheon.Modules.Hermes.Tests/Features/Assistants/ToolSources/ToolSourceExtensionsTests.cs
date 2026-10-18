using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Assistants.ToolSources;

public sealed class ToolSourceExtensionsTests
{
    [Fact]
    public void ListAllTools_WhenMultipleSources_ShouldConcatenateTheirTools()
    {
        // Arrange
        var echo = new AvailableTool(ToolSource.Native, "echo", "Echoes text.");
        var image = new AvailableTool(ToolSource.Native, "image", "Draws.");
        var search = new AvailableTool(ToolSource.Native, "search", "Searches.");
        var sources = new[] { CreateSource(echo, image), CreateSource(search) };

        // Act
        var result = sources.ListAllTools();

        // Assert
        result.ShouldBe([echo, image, search]);
    }

    [Fact]
    public void ListAllTools_WhenNoSources_ShouldReturnEmpty()
    {
        // Arrange
        var sources = Array.Empty<IHermesToolSource>();

        // Act
        var result = sources.ListAllTools();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Includes_WhenToolPresent_ShouldReturnTrue()
    {
        // Arrange
        IReadOnlyList<AvailableTool> tools = [new(ToolSource.Native, "echo", "Echoes text.")];

        // Act
        var result = tools.Includes(ToolSource.Native, "echo");

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("Echo")]
    public void Includes_WhenToolAbsent_ShouldReturnFalse(string name)
    {
        // Arrange
        IReadOnlyList<AvailableTool> tools = [new(ToolSource.Native, "echo", "Echoes text.")];

        // Act
        var result = tools.Includes(ToolSource.Native, name);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Includes_WhenSourceDiffers_ShouldReturnFalse()
    {
        // Arrange
        IReadOnlyList<AvailableTool> tools = [new(ToolSource.Native, "echo", "Echoes text.")];

        // Act
        var result = tools.Includes((ToolSource)999, "echo");

        // Assert
        result.ShouldBeFalse();
    }

    private static IHermesToolSource CreateSource(params AvailableTool[] tools)
    {
        var source = Substitute.For<IHermesToolSource>();
        source.ListTools().Returns(tools);
        return source;
    }
}
