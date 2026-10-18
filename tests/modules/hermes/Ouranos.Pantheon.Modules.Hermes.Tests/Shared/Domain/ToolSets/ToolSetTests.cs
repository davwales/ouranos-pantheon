using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.ToolSets;

public sealed class ToolSetTests
{
    private readonly Id<ToolSet> _id = new(Guid.NewGuid().ToString());

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var tools = new[] { CreateTool("echo"), CreateTool("search", "Prefer recent.") };

        // Act
        var toolSet = ToolSet.Create(_id, "Research", "Desc", "Instr", tools, isPublic: false);

        // Assert
        toolSet.Id.ShouldBe(_id);
        toolSet.Name.ShouldBe("Research");
        toolSet.Description.ShouldBe("Desc");
        toolSet.Instructions.ShouldBe("Instr");
        toolSet.IsPublic.ShouldBeFalse();
        toolSet.Tools.ShouldBe(tools);
    }

    [Fact]
    public void Create_WhenIsPublicOmitted_ShouldDefaultToPublic()
    {
        // Arrange
        var tools = Array.Empty<ToolSetTool>();

        // Act
        var toolSet = ToolSet.Create(_id, "Research", null, null, tools);

        // Assert
        toolSet.IsPublic.ShouldBeTrue();
        toolSet.Tools.ShouldBeEmpty();
    }

    [Fact]
    public void Create_WhenValuesPadded_ShouldTrimThem()
    {
        // Arrange
        var tools = Array.Empty<ToolSetTool>();

        // Act
        var toolSet = ToolSet.Create(_id, "  Research  ", "  Desc  ", "  Instr  ", tools);

        // Assert
        toolSet.Name.ShouldBe("Research");
        toolSet.Description.ShouldBe("Desc");
        toolSet.Instructions.ShouldBe("Instr");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenOptionalStringsBlank_ShouldStoreNull(string? value)
    {
        // Arrange
        var tools = Array.Empty<ToolSetTool>();

        // Act
        var toolSet = ToolSet.Create(_id, "Research", value, value, tools);

        // Assert
        toolSet.Description.ShouldBeNull();
        toolSet.Instructions.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var tools = Array.Empty<ToolSetTool>();

        // Act
        var create = () => ToolSet.Create(_id, name!, null, null, tools);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenToolsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<ToolSetTool> tools = null!;

        // Act
        var create = () => ToolSet.Create(_id, "Research", null, null, tools);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenInstructionsAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var instructions = new string('a', ToolSet.MaxInstructionsLength);

        // Act
        var toolSet = ToolSet.Create(_id, "Research", null, instructions, []);

        // Assert
        toolSet.Instructions.ShouldBe(instructions);
    }

    [Fact]
    public void Create_WhenInstructionsTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var instructions = new string('a', ToolSet.MaxInstructionsLength + 1);

        // Act
        var create = () => ToolSet.Create(_id, "Research", null, instructions, []);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenInstructionsTooLongOnlyBeforeTrimming_ShouldSucceed()
    {
        // Arrange
        var instructions = $"  {new string('a', ToolSet.MaxInstructionsLength)}  ";

        // Act
        var toolSet = ToolSet.Create(_id, "Research", null, instructions, []);

        // Assert
        toolSet.Instructions.ShouldBe(instructions.Trim());
    }

    [Fact]
    public void Create_WhenDuplicateTools_ShouldThrowArgumentException()
    {
        // Arrange
        var tools = new[] { CreateTool("echo"), CreateTool("echo", "Again.") };

        // Act
        var create = () => ToolSet.Create(_id, "Research", null, null, tools);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenToolBelongsToOtherToolSet_ShouldThrowArgumentException()
    {
        // Arrange
        var tools = new[]
        {
            CreateTool("echo", toolSetId: new Id<ToolSet>(Guid.NewGuid().ToString())),
        };

        // Act
        var create = () => ToolSet.Create(_id, "Research", null, null, tools);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", "Desc", "Instr", [], isPublic: true);

        // Act
        toolSet.Update("  Writing  ", "  New desc  ", "  New instr  ", [], isPublic: false);

        // Assert
        toolSet.Name.ShouldBe("Writing");
        toolSet.Description.ShouldBe("New desc");
        toolSet.Instructions.ShouldBe("New instr");
        toolSet.IsPublic.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WhenOptionalStringsBlank_ShouldStoreNull(string? value)
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", "Desc", "Instr", []);

        // Act
        toolSet.Update("Research", value, value, []);

        // Assert
        toolSet.Description.ShouldBeNull();
        toolSet.Instructions.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", null, null, []);

        // Act
        var update = () => toolSet.Update(name!, null, null, []);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenToolsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", null, null, []);

        // Act
        var update = () => toolSet.Update("Research", null, null, null!);

        // Assert
        update.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Update_WhenInstructionsTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", null, null, []);
        var instructions = new string('a', ToolSet.MaxInstructionsLength + 1);

        // Act
        var update = () => toolSet.Update("Research", null, instructions, []);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenDuplicateTools_ShouldThrowArgumentException()
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", null, null, []);
        var tools = new[] { CreateTool("echo"), CreateTool("echo") };

        // Act
        var update = () => toolSet.Update("Research", null, null, tools);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenToolBelongsToOtherToolSet_ShouldThrowArgumentException()
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", null, null, []);
        var tools = new[]
        {
            CreateTool("echo", toolSetId: new Id<ToolSet>(Guid.NewGuid().ToString())),
        };

        // Act
        var update = () => toolSet.Update("Research", null, null, tools);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenGuardFails_ShouldLeaveStateUnchanged()
    {
        // Arrange
        var existing = CreateTool("echo");
        var toolSet = ToolSet.Create(_id, "Research", "Desc", null, [existing]);
        var tools = new[] { CreateTool("search"), CreateTool("search") };

        // Act
        var update = () => toolSet.Update("Writing", null, null, tools);

        // Assert
        update.ShouldThrow<ArgumentException>();
        toolSet.Name.ShouldBe("Research");
        toolSet.Description.ShouldBe("Desc");
        toolSet.Tools.ShouldHaveSingleItem().ShouldBeSameAs(existing);
    }

    [Fact]
    public void Update_WhenToolAlreadyPresent_ShouldKeepExistingInstanceAndUpdateDescription()
    {
        // Arrange
        var existing = CreateTool("echo", "Old.");
        var toolSet = ToolSet.Create(_id, "Research", null, null, [existing]);
        var replacement = CreateTool("echo", "New.");

        // Act
        toolSet.Update("Research", null, null, [replacement]);

        // Assert
        var tool = toolSet.Tools.ShouldHaveSingleItem();
        tool.ShouldBeSameAs(existing);
        tool.Id.ShouldBe(existing.Id);
        tool.AdditionalDescription.ShouldBe("New.");
    }

    [Fact]
    public void Update_WhenToolsChanged_ShouldRemoveMissingAndAddNew()
    {
        // Arrange
        var kept = CreateTool("echo");
        var removed = CreateTool("search");
        var toolSet = ToolSet.Create(_id, "Research", null, null, [kept, removed]);
        var added = CreateTool("image");

        // Act
        toolSet.Update("Research", null, null, [CreateTool("echo"), added]);

        // Assert
        toolSet.Tools.Count.ShouldBe(2);
        toolSet.Tools.ShouldContain(kept);
        toolSet.Tools.ShouldContain(added);
        toolSet.Tools.ShouldNotContain(removed);
    }

    [Fact]
    public void Update_WhenToolsEmpty_ShouldRemoveAllTools()
    {
        // Arrange
        var toolSet = ToolSet.Create(_id, "Research", null, null, [CreateTool("echo")]);

        // Act
        toolSet.Update("Research", null, null, []);

        // Assert
        toolSet.Tools.ShouldBeEmpty();
    }

    private ToolSetTool CreateTool(
        string name,
        string? additionalDescription = null,
        Id<ToolSet>? toolSetId = null
    )
    {
        return ToolSetTool.Create(
            new Id<ToolSetTool>(Guid.NewGuid().ToString()),
            toolSetId ?? _id,
            ToolSource.Native,
            name,
            additionalDescription
        );
    }
}
