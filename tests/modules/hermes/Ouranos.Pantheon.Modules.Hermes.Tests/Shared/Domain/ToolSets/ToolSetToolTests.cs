using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.ToolSets;

public sealed class ToolSetToolTests
{
    private readonly Id<ToolSet> _toolSetId = new(Guid.NewGuid().ToString());

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<ToolSetTool>();

        // Act
        var tool = ToolSetTool.Create(id, _toolSetId, ToolSource.Native, "echo", "Be brief.");

        // Assert
        tool.Id.ShouldBe(id);
        tool.ToolSetId.ShouldBe(_toolSetId);
        tool.Source.ShouldBe(ToolSource.Native);
        tool.Name.ShouldBe("echo");
        tool.AdditionalDescription.ShouldBe("Be brief.");
    }

    [Fact]
    public void Create_WhenValuesPadded_ShouldTrimThem()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<ToolSetTool>();

        // Act
        var tool = ToolSetTool.Create(
            id,
            _toolSetId,
            ToolSource.Native,
            "  echo  ",
            "  Be brief.  "
        );

        // Assert
        tool.Name.ShouldBe("echo");
        tool.AdditionalDescription.ShouldBe("Be brief.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenAdditionalDescriptionBlank_ShouldStoreNull(string? additionalDescription)
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<ToolSetTool>();

        // Act
        var tool = ToolSetTool.Create(
            id,
            _toolSetId,
            ToolSource.Native,
            "echo",
            additionalDescription
        );

        // Assert
        tool.AdditionalDescription.ShouldBeNull();
    }

    [Fact]
    public void Create_WhenAdditionalDescriptionAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<ToolSetTool>();
        var additionalDescription = new string('a', ToolSetTool.MaxAdditionalDescriptionLength);

        // Act
        var tool = ToolSetTool.Create(
            id,
            _toolSetId,
            ToolSource.Native,
            "echo",
            additionalDescription
        );

        // Assert
        tool.AdditionalDescription.ShouldBe(additionalDescription);
    }

    [Fact]
    public void Create_WhenAdditionalDescriptionTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<ToolSetTool>();
        var additionalDescription = new string('a', ToolSetTool.MaxAdditionalDescriptionLength + 1);

        // Act
        var create = () =>
            ToolSetTool.Create(id, _toolSetId, ToolSource.Native, "echo", additionalDescription);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<ToolSetTool>();

        // Act
        var create = () => ToolSetTool.Create(id, _toolSetId, ToolSource.Native, name!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenSourceOutOfRange_ShouldThrowArgumentException()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<ToolSetTool>();

        // Act
        var create = () => ToolSetTool.Create(id, _toolSetId, (ToolSource)999, "echo");

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void IsSameTool_WhenSameSourceAndName_ShouldReturnTrue()
    {
        // Arrange
        var tool = CreateTool("echo");
        var other = CreateTool("echo", "Different.");

        // Act
        var result = tool.IsSameTool(other);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsSameTool_WhenNameDiffersInCase_ShouldReturnFalse()
    {
        // Arrange
        var tool = CreateTool("echo");
        var other = CreateTool("Echo");

        // Act
        var result = tool.IsSameTool(other);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsSameTool_WhenOtherNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var tool = CreateTool("echo");

        // Act
        Action isSameTool = () => tool.IsSameTool(null!);

        // Assert
        isSameTool.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldReplaceAdditionalDescription()
    {
        // Arrange
        var tool = CreateTool("echo", "Old.");

        // Act
        tool.Update("  New.  ");

        // Assert
        tool.AdditionalDescription.ShouldBe("New.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WhenAdditionalDescriptionBlank_ShouldStoreNull(string? additionalDescription)
    {
        // Arrange
        var tool = CreateTool("echo", "Old.");

        // Act
        tool.Update(additionalDescription);

        // Assert
        tool.AdditionalDescription.ShouldBeNull();
    }

    [Fact]
    public void Update_WhenAdditionalDescriptionTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var tool = CreateTool("echo");

        // Act
        var update = () =>
            tool.Update(new string('a', ToolSetTool.MaxAdditionalDescriptionLength + 1));

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    private ToolSetTool CreateTool(string name, string? additionalDescription = null)
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
