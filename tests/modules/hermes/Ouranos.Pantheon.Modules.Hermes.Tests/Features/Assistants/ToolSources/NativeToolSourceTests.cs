using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Assistants.ToolSources;

public sealed class NativeToolSourceTests
{
    private readonly Id<ToolSet> _toolSetId = new(Guid.NewGuid().ToString());

    [Fact]
    public void Constructor_WhenToolsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<IAssistantTool<HermesChatContext>> tools = null!;

        // Act
        var create = () => new NativeToolSource(tools);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Source_ShouldBeNative()
    {
        // Arrange
        var source = new NativeToolSource([]);

        // Act
        var result = source.Source;

        // Assert
        result.ShouldBe(ToolSource.Native);
    }

    [Fact]
    public void ListTools_WhenToolsRegistered_ShouldReturnNativeAvailableTools()
    {
        // Arrange
        var source = new NativeToolSource([
            CreateTool("echo", "Echoes text."),
            CreateTool("image", "Draws."),
        ]);

        // Act
        var result = source.ListTools();

        // Assert
        result.ShouldBe([
            new AvailableTool(ToolSource.Native, "echo", "Echoes text."),
            new AvailableTool(ToolSource.Native, "image", "Draws."),
        ]);
    }

    [Fact]
    public void ListTools_WhenNoTools_ShouldReturnEmpty()
    {
        // Arrange
        var source = new NativeToolSource([]);

        // Act
        var result = source.ListTools();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_WhenEntriesNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new NativeToolSource([]);

        // Act
        var resolve = () => source.Resolve(null!);

        // Assert
        resolve.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Resolve_WhenEntryMatchesTool_ShouldWrapToolWithAdditionalDescription()
    {
        // Arrange
        var echo = CreateTool("echo", "Echoes text.");
        var source = new NativeToolSource([echo, CreateTool("image", "Draws.")]);
        var entries = new[] { CreateEntry("echo", "Be brief.") };

        // Act
        var result = source.Resolve(entries);

        // Assert
        var tool = result.ShouldHaveSingleItem();
        tool.ShouldBeOfType<ToolSetAssistantTool>();
        tool.Name.ShouldBe("echo");
        tool.Description.ShouldBe("Echoes text.\n\nBe brief.");
    }

    [Fact]
    public void Resolve_WhenEntryHasNoMatchingTool_ShouldSkipIt()
    {
        // Arrange
        var source = new NativeToolSource([CreateTool("echo", "Echoes text.")]);
        var entries = new[] { CreateEntry("missing"), CreateEntry("echo") };

        // Act
        var result = source.Resolve(entries);

        // Assert
        result.ShouldHaveSingleItem().Name.ShouldBe("echo");
    }

    [Fact]
    public void Resolve_WhenNoEntries_ShouldReturnEmpty()
    {
        // Arrange
        var source = new NativeToolSource([CreateTool("echo", "Echoes text.")]);

        // Act
        var result = source.Resolve([]);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_WhenMultipleEntries_ShouldPreserveEntryOrder()
    {
        // Arrange
        var source = new NativeToolSource([
            CreateTool("echo", "Echoes text."),
            CreateTool("image", "Draws."),
        ]);
        var entries = new[] { CreateEntry("image"), CreateEntry("echo") };

        // Act
        var result = source.Resolve(entries);

        // Assert
        result.Select(t => t.Name).ShouldBe(["image", "echo"]);
    }

    private static IAssistantTool<HermesChatContext> CreateTool(string name, string description)
    {
        var tool = Substitute.For<IAssistantTool<HermesChatContext>>();
        tool.Name.Returns(name);
        tool.Description.Returns(description);
        return tool;
    }

    private ToolSetTool CreateEntry(string name, string? additionalDescription = null)
    {
        return ToolSetTool.Create(
            DatabaseExtensions.CreateId<ToolSetTool>(),
            _toolSetId,
            ToolSource.Native,
            name,
            additionalDescription
        );
    }
}
