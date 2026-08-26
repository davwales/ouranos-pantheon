using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Folders;

public sealed class FolderTests
{
    private readonly IFixture _fixture = new Fixture();

    public FolderTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<Folder>();
        var name = _fixture.Create<string>();

        // Act
        var folder = Folder.Create(id, name);

        // Assert
        folder.Id.ShouldBe(id);
        folder.Name.ShouldBe(name);
        folder.IsPublic.ShouldBeTrue();
        folder.ParentFolderId.ShouldBeNull();
        folder.ChildFolders.ShouldBeEmpty();
        folder.Conversations.ShouldBeEmpty();
    }

    [Fact]
    public void Create_WhenHappyPathWithParentFolder_ShouldSetParentFolderId()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<Folder>();
        var parentId = DatabaseExtensions.CreateId<Folder>();
        var name = _fixture.Create<string>();

        // Act
        var folder = Folder.Create(id, name, parentFolderId: parentId);

        // Assert
        folder.ParentFolderId.ShouldBe(parentId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenNameNullOrWhitespace_ShouldDefaultToNewFolder(string? name)
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<Folder>();

        // Act
        var folder = Folder.Create(id, name!);

        // Assert
        folder.Name.ShouldBe("New Folder");
    }

    [Fact]
    public void Create_WhenNameHasWhitespace_ShouldTrimName()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<Folder>();

        // Act
        var folder = Folder.Create(id, "  my folder  ");

        // Assert
        folder.Name.ShouldBe("my folder");
    }

    [Fact]
    public void Create_WhenNotPublic_ShouldSetIsPublicFalse()
    {
        // Arrange
        var id = DatabaseExtensions.CreateId<Folder>();
        var name = _fixture.Create<string>();

        // Act
        var folder = Folder.Create(id, name, isPublic: false);

        // Assert
        folder.IsPublic.ShouldBeFalse();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        var newName = _fixture.Create<string>();
        var newParentId = DatabaseExtensions.CreateId<Folder>();

        // Act
        folder.Update(newName, isPublic: true, newParentId);

        // Assert
        folder.Name.ShouldBe(newName);
        folder.IsPublic.ShouldBeTrue();
        folder.ParentFolderId.ShouldBe(newParentId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenNameNullOrWhitespace_ShouldDefaultToNewFolder(string? newName)
    {
        // Arrange
        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );

        // Act
        folder.Update(newName!, isPublic: true, null);

        // Assert
        folder.Name.ShouldBe("New Folder");
    }
}
