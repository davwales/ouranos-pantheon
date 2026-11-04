using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Folders;

public sealed class FolderTreeQueryServiceTests
{
    [Fact]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new FolderTreeQueryService(null!));
    }
}
